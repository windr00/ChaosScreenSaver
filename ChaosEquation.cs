using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Color = SharpDX.Color;
namespace ChaosScreen
{
    public partial class ScreenSaverForm
    {
        [StructLayout(LayoutKind.Sequential)]
        struct Vertex
        {
            public Vector2 position;
            public SharpDX.ColorBGRA color;

            public float x
            {
                get
                {
                    return position.X;
                }
                set
                {
                    position.X = value;
                }
            }

            public float y
            {
                get
                {
                    return position.Y;
                }
                set
                {
                    position.Y = value;
                }
            }

        }
        private  Device device;


        private const int iterations = 600;
        private const int steps = 400;
        private const double deltaPerStep = 1e-5;
        private const double deltaMinimum = 1e-7;
        private  VertexBuffer gpuVertices;
        private  Vertex[] vertexArray;
        private  VertexDeclaration vertexDecl;
        private  float plotScale = 0.25f;
        private  float plotX = 0.0f;
        private  float plotY = 0.0f;
        private  double t = double.MaxValue;
        private  int width;
        private  int height;

        private  VertexBuffer gpuRectangle;
        private  VertexDeclaration rectangleDecl;

        private const double tStart = -3.0;
        private const double tEnd = 3.0;
        private  string equationCode = "";
        public  void Initialize()
        {
            var parameter = new PresentParameters(form.ClientSize.Width, form.ClientSize.Height);
            parameter.MultiSampleType = MultisampleType.EightSamples;
            parameter.PresentationInterval = PresentInterval.One;
            width = form.Width;
            height = form.Height;
            device = new Device(new Direct3D(), 0, DeviceType.Hardware, form.Handle, CreateFlags.HardwareVertexProcessing,
                parameter);
            gpuVertices = new VertexBuffer(device, iterations * steps * 12, Usage.WriteOnly, VertexFormat.None, Pool.Managed);
            vertexArray = new Vertex[iterations * steps];
            gpuVertices.Lock(0, 0, LockFlags.None).WriteRange(vertexArray);
            gpuVertices.Unlock();
            gpuRectangle = new VertexBuffer(device, 6 * 12, Usage.WriteOnly, VertexFormat.None, Pool.Managed);
            
            device.SetRenderState(RenderState.PointSize, 2.0f);
            device.SetRenderState(RenderState.MultisampleAntialias, true);
            device.SetRenderState(RenderState.AlphaBlendEnable, true);

            device.SetRenderState(RenderState.SourceBlend, Blend.One);//D3DBLEND_ONE
            device.SetRenderState(RenderState.DestinationBlend, Blend.One);//D3DBLEND_ONE
            device.SetRenderState(RenderState.BlendOperation, BlendOperation.ReverseSubtract);
            device.SetRenderState(RenderState.SourceBlendAlpha, Blend.One);//D3DBLEND_ONE
            device.SetRenderState(RenderState.DestinationBlendAlpha, Blend.One);//D3DBLEND_ONE
            device.SetRenderState(RenderState.BlendOperationAlpha, BlendOperation.ReverseSubtract);
            var vertexElems = new[] {
                new VertexElement(0, 0, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.PositionTransformed, 0),
                new VertexElement(0, 8, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0),
                VertexElement.VertexDeclarationEnd
            };
            rectangleDecl = new VertexDeclaration(device, vertexElems);
            vertexDecl = new VertexDeclaration(device, vertexElems);
        }

         void RandParams(ref double[] parameters)
        {
            var unix = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var rand_int = new Random(unix);
            for (int i = 0; i < parameters.Length; i++)
            {
                int r = rand_int.Next(0, 3);
                if (r == 0)
                {
                    parameters[i] = 1.0f;
                }
                else if (r == 1)
                {
                    parameters[i] = -1.0f;
                }
                else
                {
                    parameters[i] = 0.0f;
                }
            }
        }

         string ParamsToString(ref double[] parameters)
        {
            char[] base27 = "_ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            int a = 0;
            int n = 0;
            string result = "";
            for (int i = 0; i < parameters.Length; i++)
            {
                a = a * 3 + (int)(parameters[i]) + 1;
                n += 1;
                if (n == 3)
                {
                    result += base27[a];
                    a = 0;
                    n = 0;
                }
            }
            return result;
        }

         void SIGN_OR_SKIP(int i, string x, ref StringBuilder ss, ref bool isFirst, double[] parameters)
        {
            if (parameters[i] != 0.0)
            {
                if (isFirst)
                {
                    if (parameters[i] == -1.0)
                    {
                        ss.Append('-');
                    }
                }
                else
                {
                    if (parameters[i] == -1.0)
                    {
                        ss.Append(" - ");
                    }
                    else
                    {
                        ss.Append(" + ");
                    }
                }
                ss.Append(x);
                isFirst = false;
            }
        }

         string MakeEquationString(double[] parameters)
        {
            StringBuilder ss = new StringBuilder();
            bool isFirst = true;
            SIGN_OR_SKIP(0, "x\u00b2", ref ss, ref isFirst, parameters);
            SIGN_OR_SKIP(1, "y\u00b2", ref ss, ref isFirst, parameters);
            SIGN_OR_SKIP(2, "t\u00b2", ref ss, ref isFirst, parameters);
            SIGN_OR_SKIP(3, "xy", ref ss, ref isFirst, parameters);
            SIGN_OR_SKIP(4, "xt", ref ss, ref isFirst, parameters);
            SIGN_OR_SKIP(5, "yt", ref ss, ref isFirst, parameters);
            SIGN_OR_SKIP(6, "x", ref ss, ref isFirst, parameters);
            SIGN_OR_SKIP(7, "y", ref ss, ref isFirst, parameters);
            SIGN_OR_SKIP(8, "t", ref ss, ref isFirst, parameters);

            return ss.ToString();
        }

         void GenerateNew(out double t, ref double[] parameters)
        {
            var unix = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var rand_int = new Random(unix);
            var color = new Color(rand_int.Next(2, 10), rand_int.Next(2, 10), rand_int.Next(2, 10), 0);
            gpuRectangle.Lock(0, 0, LockFlags.None).WriteRange(new[]
            {
                new Vertex{ position = new Vector2(-1, -1), color = color},
                new Vertex{ position = new Vector2(width + 1, 0), color = color},
                new Vertex{ position = new Vector2(0, height + 1), color = color},
                new Vertex{ position = new Vector2(0, height + 1), color = color},
                new Vertex{ position = new Vector2(width + 1, 0), color = color},
                new Vertex{ position = new Vector2(width + 1, height + 1), color = color},

            });
            gpuRectangle.Unlock();
            t = tStart;
            equationCode = ParamsToString(ref parameters);
            string equationString = "x' = " + MakeEquationString(parameters) + "\n" +
                                    "y' = " + MakeEquationString(parameters.Skip(parameters.Length / 2).ToArray()) + "\n" +
                                    "Code: " + equationCode;
            Console.WriteLine(equationString);
        }

         void ResetPlot()
        {
            plotScale = 0.25f;
            plotX = 0.0f;
            plotY = 0.0f;
        }

        private  Color GetRandColor(int i)
        {
            i += 1;
            int r = Math.Min(255, 50 + (i * 11909) % 256);
            int g = Math.Min(255, 50 + (i * 52973) % 256);
            int b = Math.Min(255, 50 + (i * 44111) % 256);
            return new Color(r, g, b, 16);
        }

        private  Vector2 ToScreen(double x, double y)
        {
            float s = plotScale * (float)(height / 2);
            float nx = (float)width * 0.5f + ((float)x - plotX) * s;
            float ny = (float)height * 0.5f + ((float)y - plotY) * s;
            return new Vector2(nx, ny);
        }

        public  void Render()
        {
            double[] parameters = new double[18];
            for (int i = 0; i < vertexArray.Length; i++)
            {
                vertexArray[i].color = GetRandColor(i % iterations);
            }
            double rollingDelta = deltaPerStep;
            List<Vector2> history = new List<Vector2>(iterations);
            for (int i = 0; i < iterations; i++)
            {
                history.Add(new Vector2());
            }
            RenderLoop.Run(form, () =>
            {
                if (t > tEnd)
                {
                    ResetPlot();
                    RandParams(ref parameters);
                    GenerateNew(out t, ref parameters);
                }

                //TODO : fade colors;
                device.BeginScene();
                device.SetRenderState(RenderState.SourceBlendAlpha, Blend.One);//D3DBLEND_ONE
                device.SetRenderState(RenderState.DestinationBlendAlpha, Blend.One);//D3DBLEND_ONE
                device.SetRenderState(RenderState.BlendOperationAlpha, BlendOperation.ReverseSubtract);
                device.SetRenderState(RenderState.SourceBlend, Blend.One);//D3DBLEND_ONE
                device.SetRenderState(RenderState.DestinationBlend, Blend.One);//D3DBLEND_ONE
                device.SetRenderState(RenderState.BlendOperation, BlendOperation.ReverseSubtract);
                device.SetStreamSource(0, gpuRectangle, 0, 12);
                device.VertexDeclaration = rectangleDecl;
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                double delta = deltaPerStep;
                rollingDelta = rollingDelta * 0.99 + delta * 0.01;
                for (int step = 0; step < steps; step++)
                {
                    bool isOffScreen = true;
                    double x = t;
                    double y = t;
                    for (int iter = 0; iter < iterations; ++iter)
                    {
                        double xx = x * x;
                        double yy = y * y;
                        double tt = t * t;
                        double xy = x * y;
                        double xt = x * t;
                        double yt = y * t;
                        double nx = xx * parameters[0] + yy * parameters[1] + tt * parameters[2] + xy * parameters[3] + xt * parameters[4] + yt * parameters[5] + x * parameters[6] + y * parameters[7] + t * parameters[8];
                        double ny = xx * parameters[9] + yy * parameters[10] + tt * parameters[11] + xy * parameters[12] + xt * parameters[13] + yt * parameters[14] + x * parameters[15] + y * parameters[16] + t * parameters[17];
                        x = nx;
                        y = ny;
                        Vector2 screenPt = ToScreen(x, y);
                        vertexArray[step * iterations + iter].position = screenPt;
                        if (screenPt.X > 0.0f && screenPt.Y > 0.0f && screenPt.X < width && screenPt.Y < height)
                        {
                            float dx = history[iter].X - (float)x;
                            float dy = history[iter].Y - (float)y;
                            double dist = (double)(500.0f * Math.Sqrt(dx * dx + dy * dy));
                            
                            if (!double.IsNaN(dist))
                            {
                                rollingDelta = Math.Min(rollingDelta, Math.Max(delta / (dist + 1e-5), deltaMinimum));
                            }
                            isOffScreen = false;
                        }
                        history[iter] = new Vector2((float)x, (float)y);

                    }
                    if (isOffScreen)
                    {
                        t += 0.01;
                    }
                    else
                    {
                        t += rollingDelta;
                    }
                }


                gpuVertices.Lock(0, 0, LockFlags.None).WriteRange(vertexArray);
                gpuVertices.Unlock();
                device.SetRenderState(RenderState.SourceBlend, Blend.One);//D3DBLEND_ONE
                device.SetRenderState(RenderState.DestinationBlend, Blend.One);//D3DBLEND_ONE
                device.SetRenderState(RenderState.BlendOperation, BlendOperation.Maximum);

                device.SetStreamSource(0, gpuVertices, 0, 12);
                device.VertexDeclaration = vertexDecl;
                device.DrawPrimitives(PrimitiveType.PointList, 0, vertexArray.Length);
                device.EndScene();
                device.Present();
            });
        }
    }
}
