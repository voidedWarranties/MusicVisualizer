using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;

namespace MusicVisualizer.Game.UI
{
    [Cached]
    public class VisualizerContainer : Container
    {
        public Bindable<Track> Track = new Bindable<Track>();

        private const int size = ChannelAmplitudes.AMPLITUDES_SIZE;

        private const float smoothing = 0.5f;

        public float[] SmoothedAmplitudes { get; } = new float[size];

        public float[] TemporalAmplitudes { get; } = new float[size];

        private float[] prevAmplitudes = new float[size];

        public float Activity = 0.0f;

        public float ActivityDelta = 0;

        private List<float> activeHistory = new List<float>();

        public float[] Delta = new float[size];

        private void updateAmplitudes()
        {
            prevAmplitudes = (float[])SmoothedAmplitudes.Clone();

            for (int i = 0; i < size; i++)
                TemporalAmplitudes[i] = 0;

            if (Track.Value != null)
                addAmplitudesFromSource(Track.Value);

            for (int i = 0; i < size; i++)
                SmoothedAmplitudes[i] = smoothing * prevAmplitudes[i] + (1 - smoothing) * TemporalAmplitudes[i];

            Activity = 0.0f;
            int num2 = 64;
            for (int i = 0; i < num2; i++)
                Activity += 2f * TemporalAmplitudes[i] * (num2 - i) / num2;

            activeHistory.Add(Activity);
            if (activeHistory.Count > 16) activeHistory.RemoveAt(0);

            ActivityDelta = Activity - activeHistory.Sum() / activeHistory.Count;

            for (var i = 0; i < size; i++)
            {
                Delta[i] = TemporalAmplitudes[i] - prevAmplitudes[i];
            }
        }

        private void addAmplitudesFromSource(IHasAmplitudes source)
        {
            var amplitudes = source.CurrentAmplitudes.FrequencyAmplitudes.Span;

            for (int i = 0; i < amplitudes.Length; i++)
            {
                if (i < SmoothedAmplitudes.Length)
                    TemporalAmplitudes[i] += amplitudes[i];
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var delayed = Scheduler.AddDelayed(updateAmplitudes, 25, true);
            delayed.PerformRepeatCatchUpExecutions = false;

            Track.ValueChanged += _ =>
            {
                activeHistory.Clear();
            };
        }
    }
}
