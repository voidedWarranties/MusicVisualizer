using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osuTK;
using osuTK.Graphics;

namespace MusicVisualizer.Game.UI
{
    public class BarVisualizer : Drawable
    {
        private IShader shader;

        [Resolved]
        private VisualizerContainer vis { get; set; }

        protected override void Update()
        {
            base.Update();
        }

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders)
        {
            shader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE);
        }

        protected override DrawNode CreateDrawNode() => new DebugVisualiserDrawNode(this);

        private class DebugVisualiserDrawNode : DrawNode
        {
            protected new BarVisualizer Source => (BarVisualizer)base.Source;

            private float[] audioData;

            private Vector2 size;

            private IShader shader;

            public DebugVisualiserDrawNode(BarVisualizer source)
                : base(source)
            {
            }

            public override void ApplyState()
            {
                base.ApplyState();

                audioData = Source.vis.SmoothedAmplitudes;
                size = Source.ToScreenSpace(Source.DrawSize);
                shader = Source.shader;
            }

            public override void Draw(Action<TexturedVertex2D> vertexAction)
            {
                base.Draw(vertexAction);

                shader.Bind();

                float width = size.X / audioData.Length / 2;

                for (int i = 0; i < audioData.Length; i++)
                {
                    float height = audioData[i] * 2 * size.Y;

                    var rectangle = new Quad(i * width, size.Y - height, width, height);

                    DrawQuad(Texture.WhitePixel, rectangle, new Color4(255, 255, 255, 200));
                }

                shader.Unbind();
            }
        }
    }
}
