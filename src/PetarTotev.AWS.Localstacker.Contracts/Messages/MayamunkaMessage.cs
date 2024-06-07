using System;
using System.Runtime.Serialization;
using System.Text;

namespace PetarTotev.AWS.Localstacker.Contracts.Messages
{
    [DataContract]
    public class MayamunkaMessage
    {
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public int Coins { get; set; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"{nameof(FirstName)}: {FirstName}");
            stringBuilder.AppendLine($"{nameof(LastName)}: {LastName}");
            stringBuilder.AppendLine($"{nameof(Email)}: {Email}");
            stringBuilder.AppendLine($"{nameof(Coins)}: {Coins}");

            return stringBuilder.ToString();
        }
    }
}
