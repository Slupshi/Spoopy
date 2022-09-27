using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spoopy.Models
{
    public class SpoopyRole
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int MemberCount { get; set; }
        public DateTime CreatedAt { get; set; }

        public SpoopyRole(string name, int memberCount, DateTime createdAt)
        {
            Name = name;
            MemberCount = memberCount;
            CreatedAt = createdAt;
        }
    }
}
