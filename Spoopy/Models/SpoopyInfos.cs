using System;
using Discord;

namespace Spoopy.Models
{
    public class SpoopyInfos
    {
        public string Name { get; set; }
        public string AvatarURL { get; set; }
        public IUser Owner { get; set; }
        public DateTime CreatedAt { get; set; }

        public SpoopyInfos(string name, string avatarUrl, IUser owner, DateTime createdAt)
        {
            Name = name;
            AvatarURL = avatarUrl;
            Owner = owner;
            CreatedAt = createdAt;
        }
    }
}
