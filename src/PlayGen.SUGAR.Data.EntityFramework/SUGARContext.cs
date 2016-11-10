﻿using System;
using System.Linq;
using PlayGen.SUGAR.Data.Model;
using PlayGen.SUGAR.Data.Model.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace PlayGen.SUGAR.Data.EntityFramework
{
	/// <summary>
	/// Entity Framework Database Configuration
	/// </summary>
	public class SUGARContext : DbContext
	{
		private readonly bool _isSaveDisabled;

		internal SUGARContext(DbContextOptions<SUGARContext> options, bool disableSave = false) : base(options)
		{
			_isSaveDisabled = disableSave;
		}
		
		public DbSet<Account> Accounts { get; set; }

		public DbSet<Game> Games { get; set; }

		public DbSet<GameData> GameData { get; set; }
		
		public DbSet<Evaluation> Evaluations { get; set; }
		public DbSet<Achievement> Achievements { get; set; }
		public DbSet<Skill> Skills { get; set; }

		public DbSet<Actor> Actors { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<Group> Groups { get; set; }

		public DbSet<UserToUserRelationshipRequest> UserToUserRelationshipRequests { get; set; }
		public DbSet<UserToUserRelationship> UserToUserRelationships { get; set; }
		public DbSet<UserToGroupRelationshipRequest> UserToGroupRelationshipRequests { get; set; }
		public DbSet<UserToGroupRelationship> UserToGroupRelationships { get; set; }

		public DbSet<Leaderboard> Leaderboards { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ConfigureTableNames();
			modelBuilder.ConfigureHierarchy();
			modelBuilder.ConfigureCompositePrimaryKeys();
			modelBuilder.ConfigureIndexes();
			modelBuilder.ConfigureForeignKeys();
			modelBuilder.ConfigureProperties();
		}

		public override int SaveChanges()
		{
			UpdateModificationHistory();

			return _isSaveDisabled 
				? 0
				: base.SaveChanges();
		}

		/// <summary>
		/// User reflection to detect classes that implement the IModificationHistory interface
		/// and set their DateCreated and DateModified DateTime fields accordingly.
		/// </summary>
		private void UpdateModificationHistory()
		{
			var histories = this.ChangeTracker.Entries()
				.Where(e => e.Entity is IModificationHistory && (e.State == EntityState.Added || e.State == EntityState.Modified))
				.Select(e => e.Entity as IModificationHistory);

			foreach (var history in histories)
			{
				history.DateModified = DateTime.Now;

				if (history.DateCreated == default(DateTime))
				{
					history.DateCreated = DateTime.Now;
				}
			}
		}
	}
}