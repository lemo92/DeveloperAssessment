using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Infrastructure
{
    public static class DbInitializer
    {
        public static void MigrateAndSeed(AppDbContext context)
        {
            context.Database.Migrate();

            
            if (!context.Users.Any())
            {
                var admin = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "admin",
                    Email = "admin@example.com",
                    PasswordHash = "changeme", 
                    Role = "Admin"
                };

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "jane",
                    Email = "jane@example.com",
                    PasswordHash = "changeme",
                    Role = "User"
                };

                context.Users.AddRange(admin, user);
                context.SaveChanges(); 
            }

           
            if (!context.Tasks.Any())
            {
                var assignedUser = context.Users.First(); 
                context.Tasks.AddRange(
                    new TaskItem
                    {
                        Id = Guid.NewGuid(),
                        Title = "Sample task",
                        Description = "This is a seeded task",
                        Status = TaskTracker.Domain.Entities.TaskStatus.New,
                        DueDate = DateTime.UtcNow.AddDays(3),
                        AssignedUserId = assignedUser.Id
                    },
                    new TaskItem
                    {
                        Id = Guid.NewGuid(),
                        Title = "Overdue example",
                        Description = "This will show overdue handling",
                        Status = TaskTracker.Domain.Entities.TaskStatus.New,
                        DueDate = DateTime.UtcNow.AddDays(-1),
                        AssignedUserId = assignedUser.Id 
                    }
                );
                context.SaveChanges();
            }
        }
    }
}

