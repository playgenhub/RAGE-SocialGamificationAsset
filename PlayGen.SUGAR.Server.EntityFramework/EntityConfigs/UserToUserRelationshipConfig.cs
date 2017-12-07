﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayGen.SUGAR.Server.Model;

namespace PlayGen.SUGAR.Server.EntityFramework.EntityConfigs
{
    public class UserToUserRelationshipConfig : IEntityTypeConfiguration<UserToUserRelationship>
	{
		public void Configure(EntityTypeBuilder<UserToUserRelationship> builder)
		{
			builder.HasOne(u => u.Requestor)
				.WithMany(u => u.Requestors)
				.HasForeignKey(u => u.RequestorId)
				.IsRequired();

			builder.HasOne(u => u.Acceptor)
				.WithMany(u => u.Acceptors)
				.HasForeignKey(u => u.AcceptorId)
				.IsRequired();

			builder.HasKey(k => new { k.RequestorId, k.AcceptorId });
		}
	}
}
