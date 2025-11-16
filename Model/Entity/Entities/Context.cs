using Microsoft.EntityFrameworkCore;

namespace Entity.Entities;

public partial class Context : DbContext
{
    private readonly string _connectionString;
    public Context(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Context(DbContextOptions<Context> options)
        : base(options)
    {
    }

    public virtual DbSet<SysUser> TmsUsers { get; set; }
    public virtual DbSet<Device> Devices { get; set; }
    public virtual DbSet<SysMediaCollection> SysMediaCollections { get; set; }
    public virtual DbSet<SysMedia> SysMedias { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https: //go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
    {
        optionsBuilder.UseSqlServer(_connectionString);
        base.OnConfiguring(optionsBuilder);
    } 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Device>().HasOne(d => d.User).WithMany(p => p.Devices)
            .HasForeignKey(d => d.UserId);

        modelBuilder.Entity<SysMedia>().HasOne(d => d.MediaCollection).WithMany(p => p.Medias)
            .HasForeignKey(d => d.MediaCollectionId);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
