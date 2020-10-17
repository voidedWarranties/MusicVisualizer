using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;

namespace MusicVisualizer.Game.UI
{
    public class TriangleVisualizer : Container
    {
        [Resolved]
        private VisualizerContainer vis { get; set; }

        [Resolved]
        private BackgroundVideo video { get; set; }

        private int targetTriangles => (int)DrawWidth / 4;

        private readonly List<Triangle> triangles = new List<Triangle>();

        private readonly Random rng = new Random();

        private int baseVelocity = 1;

        private float velocity = 0;

        public Color4[] Colors = new Color4[]
        {
            Color4.White
        };

        private void addTriangles(bool randVelocity = true)
        {
            for (var i = triangles.Count; i < targetTriangles; i++)
            {
                var triangle = new Triangle();

                resetTriangle(triangle);

                if (randVelocity)
                {
                    velocity = DrawHeight / 9;
                }
                else
                {
                    triangle.X = (float)rng.NextDouble() * DrawWidth;
                    triangle.Y = (float)rng.NextDouble() * DrawHeight;

                    triangle.Hide();
                    triangle.FadeIn(300, Easing.InQuint);
                }

                triangles.Add(triangle);
            }
        }

        private void resetTriangle(Triangle t)
        {
            if (triangles.Count > targetTriangles)
            {
                removeTriangle(t);
                return;
            }

            var size = rng.Next(25, 150);

            t.Size = new Vector2(size);
            t.Anchor = Anchor.TopLeft;
            t.Origin = Anchor.Centre;

            t.Colour = Colors[rng.Next(Colors.Length)];

            t.X = (float)rng.NextDouble() * DrawWidth;
            t.Y = DrawHeight + t.DrawHeight;

            Remove(t);
            Add(t);
        }

        protected override void Update()
        {
            base.Update();

            float clockFactor = (float)Clock.ElapsedFrameTime / (1000 / 60);

            velocity = Math.Max(velocity, (float)Math.Min(vis.Activity * 3, 15));

            velocity -= velocity * 0.05f * clockFactor;

            float num = (float)((baseVelocity + velocity) * clockFactor * 0.5);

            for (int i = 0; i < triangles.Count; i++)
            {
                var triangle = triangles[i];
                float scaleFactor = triangle.DrawWidth / 700;
                triangle.Y -= (float)(num * (0.2 + 8 * scaleFactor));
                if (triangle.Y < -triangle.DrawHeight)
                    resetTriangle(triangle);
            }

            if (triangles.Count < targetTriangles)
                addTriangles(false);
        }

        public void UpdateColors(Color4[] colors, bool fadeColor)
        {
            Colors = colors;

            if (fadeColor)
            {
                foreach (var t in triangles)
                {
                    t.ClearTransforms();
                    t.FadeColour(Colors[rng.Next(Colors.Length)], 300, Easing.InQuad);
                }
            }
        }

        private void removeTriangle(Triangle t)
        {
            Remove(t);
            triangles.Remove(t);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            addTriangles();

            video.Colors.ValueChanged += ev =>
            {
                // do transforms on update thread - https://github.com/ppy/osu-framework/issues/1746
                Scheduler.Add(() =>
                {
                    UpdateColors(ev.NewValue.Colors, ev.NewValue.Initial);
                });
            };
        }
    }
}
