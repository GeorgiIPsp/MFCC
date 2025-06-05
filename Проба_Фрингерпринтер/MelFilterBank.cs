namespace Mfccextractor
{
    internal class MelFilterBank
    {
        private int targetSampleRate;
        private int frameSize;
        private int melFilterCount;
        private object lowerFreq;
        private object upperFreq;

        public MelFilterBank(int targetSampleRate, int frameSize, int melFilterCount, object lowerFreq, object upperFreq)
        {
            this.targetSampleRate = targetSampleRate;
            this.frameSize = frameSize;
            this.melFilterCount = melFilterCount;
            this.lowerFreq = lowerFreq;
            this.upperFreq = upperFreq;
        }
    }
}