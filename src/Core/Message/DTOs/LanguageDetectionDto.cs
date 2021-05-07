using System;

namespace Core.Message.DTOs
{
    public class LanguageDetectionDto
    {
        public int Time { get; set; }
        public LanguageDetectionItem[] DetectedLangs { get; set; }
        public DateTime Timestamp { get; set; }
    }


    public class LanguageDetectionItem
    {
        public string Lang { get; set; }
        public decimal Confidence { get; set; }
    }
}
