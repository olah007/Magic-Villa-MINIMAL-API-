﻿using MagicVilla_CouponAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_CouponAPI.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {

        }

        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<LocalUser> LocalUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Coupon>().HasData(
                new Coupon()
                {
                    Id = 1,
                    Name = "100FF",
                    Percent = 10,
                    IsActive = true,
                },
                new Coupon()
                {
                    Id = 2,
                    Name = "200FF",
                    Percent = 20,
                    IsActive = true,
                }
            );
        }
    }
}
