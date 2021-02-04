using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;

namespace MusicVisualizer.Game.UI.Visualizers
{
    public class ProjectileVisualizer : Container
    {
        [Resolved]
        private VisualizerContainer vis { get; set; }

        [Resolved]
        private BackgroundVideo backgroundVideo { get; set; }

        private readonly Random rng = new Random();

        public Color4[] Colors =
        {
            Color4.White
        };

        private readonly List<Projectile> projectiles = new List<Projectile>();

        private void createSprite(Vector2 pos, Vector2 direction, float velocity, Color4 color, float gravity = 0)
        {
            var circle = new Projectile
            {
                Position = pos,
                Direction = direction,
                Velocity = velocity,
                Gravity = gravity,
                Colour = color,
                Size = new Vector2(12)
            };

            Add(circle);
            projectiles.Add(circle);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            backgroundVideo.Colors.ValueChanged += ev =>
            {
                Colors = ev.NewValue.Colors;
            };
        }

        protected override void Update()
        {
            base.Update();

            var deltaFiltered = vis.Delta.Take((int)(vis.Delta.Length * 0.75)).Where(d => d > 0.005).ToList();

            var origin = new Vector2(rng.Next((int)DrawWidth), rng.Next((int)DrawHeight));
            var color = Colors[rng.Next(Colors.Length)];
            var offset = (float)(rng.NextDouble() * 2 * Math.PI);

            if (deltaFiltered.Count > 50)
            {
                var avg = deltaFiltered.Sum() / deltaFiltered.Count;
                var sides = (int)Math.Floor((double)8 * deltaFiltered.Count / vis.Delta.Length) + 3;

                var angles = getPolygonAngles(offset, sides, Math.Min(deltaFiltered.Count * 2, 100));

                foreach (var angle in angles)
                {
                    var velocity = ((100 * (avg / 0.2f)) + 5) / 2;
                    createSprite(origin, angle, velocity, color);
                }
            }
            else if (deltaFiltered.Count < 10)
            {
                for (var i = 0; i < vis.Delta.Length * 0.75; i++)
                {
                    var delta = vis.Delta[i];
                    var ratio = delta / 0.2;

                    if (ratio > 0.01)
                    {
                        var angle = (i / (vis.Delta.Length * 0.75) * 2 * Math.PI) + offset;
                        var velocity = ((100 * ratio) + 5) / 2;

                        createSprite(origin, new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)), (float)velocity, color);
                    }
                }
            }

            float clockFactor = (float)Clock.ElapsedFrameTime / (1000 / 60f);

            foreach (var p in projectiles)
            {
                p.Position += p.Direction * p.Velocity * clockFactor;
                p.Y += p.Gravity;

                if (p.Gravity > 0) p.Gravity += 0.01f * clockFactor;

                p.Velocity -= p.Velocity * 0.03f * clockFactor;

                if (p.Velocity < 5 && !p.IsRemoving)
                {
                    p.FadeOut(750, Easing.OutQuad).Expire();

                    p.IsRemoving = true;
                }
            }
        }

        private IEnumerable<Vector2> getPolygonPoints(int sides)
        {
            var points = new List<Vector2>();

            for (float deg = 0; Math.Round(deg) < 360; deg += 360f / sides)
            {
                float angle = (float)(deg * Math.PI / 180);
                points.Add(new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)));
            }

            return points;
        }

        private IEnumerable<Vector2> getPolygonAngles(float offset, int sides, int projectiles)
        {
            var angles = new List<Vector2>();
            var points = getPolygonPoints(sides).ToArray();

            var sideLen = Vector2Extensions.Distance(points[0], points[1]);
            var space = sideLen / ((float)projectiles / sides);

            for (var i = 0; i < points.Length; i++)
            {
                var point = points[i];
                var nextPoint = points[(i + 1) % points.Length];

                var angle = Math.Atan2(nextPoint.Y - point.Y, nextPoint.X - point.X);
                var xT = Math.Cos(angle) * space;
                var yT = Math.Sin(angle) * space;

                for (var j = 0; j < projectiles / sides; j++)
                {
                    var x = point.X + xT * j;
                    var y = point.Y + yT * j;

                    var xRot = x * Math.Cos(offset) - y * Math.Sin(offset);
                    var yRot = x * Math.Sin(offset) + y * Math.Cos(offset);

                    angles.Add(new Vector2((float)xRot, (float)yRot));
                }
            }

            return angles;
        }

        private class Projectile : Circle
        {
            public Vector2 Direction { get; set; }

            public float Velocity { get; set; }

            public float Gravity { get; set; }

            public bool IsRemoving { get; set; }
        }
    }
}
