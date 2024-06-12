using System;

namespace PetarTotev.AWS.Localstacker.Contracts.Messages
{
    public class PeterRabbitMessage
    {
        public string Author { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Content { get; set; }
        public DateTime DatePublished { get; set; }
        public bool IsInEnglish { get; set; }
    }
}
