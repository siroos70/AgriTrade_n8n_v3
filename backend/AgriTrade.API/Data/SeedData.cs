using AgriTrade.Backend.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace AgriTrade.Backend.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider services)
        {
            var context = services.GetRequiredService<AgriTradeContext>();
            await context.Database.EnsureCreatedAsync();

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<User>>();

            string[] roleNames = { "Admin", "Operator", "Seller", "Buyer", "Accountant", "Viewer" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole { Name = roleName });
                }
            }

            // Admin user
            var adminUser = new User
            {
                UserName = "admin@agritrade.com",
                Email = "admin@agritrade.com",
                Name = "مدیر سیستم",
                Role = "Admin",
                Active = true,
                CreatedAt = DateTime.Now
            };

            if (await userManager.FindByNameAsync(adminUser.UserName) == null)
            {
                var result = await userManager.CreateAsync(adminUser, "admin123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Sample seller
            var sellerUser = new User
            {
                UserName = "seller@agritrade.com",
                Email = "seller@agritrade.com",
                Name = "علی فروشنده",
                Role = "Seller",
                Active = true,
                CreatedAt = DateTime.Now
            };

            if (await userManager.FindByNameAsync(sellerUser.UserName) == null)
            {
                var result = await userManager.CreateAsync(sellerUser, "seller123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(sellerUser, "Seller");
                }
            }

            // Sample buyer
            var buyerUser = new User
            {
                UserName = "buyer@agritrade.com",
                Email = "buyer@agritrade.com",
                Name = "محمد خریدار",
                Role = "Buyer",
                Active = true,
                CreatedAt = DateTime.Now
            };

            if (await userManager.FindByNameAsync(buyerUser.UserName) == null)
            {
                var result = await userManager.CreateAsync(buyerUser, "buyer123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(buyerUser, "Buyer");
                }
            }
        }
    }
}