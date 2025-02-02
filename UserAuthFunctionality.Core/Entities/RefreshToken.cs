using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserAuthFunctionality.Core.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
        public string Token {  get; set; }
        public DateTime Expires { get; set; }
        public bool IsExpired { get; set; }
        public bool IsActive { get; set; }
        public DateTime? Revoked { get; set; }
        public Task UpdateStatus()
        {
            IsExpired = DateTime.UtcNow >= Expires;
            IsActive = !IsExpired && Revoked == null;
         return Task.CompletedTask;
        }
    }
}
