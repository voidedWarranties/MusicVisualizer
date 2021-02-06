using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Textures;
using osuTK;
using osuTK.Graphics;

namespace MusicVisualizer.Game.UI.Visualizers
{
    public class BarVisualizer : Drawable
    {
        [Resolved]
        private VisualizerContainer vis { get; set; }

        protected override DrawNode CreateDrawNode() => new BarVisualizerDrawNode(this);

        private class BarVisualizerDrawNode : DrawNode
        {
            protected new BarVisualizer Source => (BarVisualizer)base.Source;

            private float[] audioData;

            private Vector2 size;

            public BarVisualizerDrawNode(IDrawable source)
                : base(source)
            {
            }

            public override void ApplyState()
            {
                base.ApplyState();

                audioData = Source.vis.SmoothedAmplitudes;
                size = Source.ScreenSpaceDrawQuad.Size;
            }

            public override void Draw(Action<TexturedVertex2D> vertexAction)
            {
                base.Draw(vertexAction);

                float width = size.X / audioData.Length / 2;

                for (int i = 0; i < audioData.Length; i++)
                {
                    float height = audioData[i] * 2 * size.Y;

                    var rectangle = new Quad(i * width, size.Y - height, width, height);

                    DrawQuad(Texture.WhitePixel, rectangle, new Color4(255, 255, 255, 200));
                }
            }
        }
    }
}
