// <auto-generated />
using System;
using LibraryX.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LibraryX.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250527000001_AddMissingIdentityTables")]
    partial class AddMissingIdentityTables
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0");

            // No need to define the full model here since this is just for fixing missing tables
#pragma warning restore 612, 618
        }
    }
}
