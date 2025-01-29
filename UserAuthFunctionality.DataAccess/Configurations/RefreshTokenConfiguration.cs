using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserAuthFunctionality.Core.Entities;

namespace UserAuthFunctionality.DataAccess.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.Property(s => s.Id).IsRequired();
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Token).IsRequired();
            builder.HasIndex(s=>s.Token).IsUnique();
            builder.Property(s => s.Token).HasMaxLength(250);
        }
    }
}
