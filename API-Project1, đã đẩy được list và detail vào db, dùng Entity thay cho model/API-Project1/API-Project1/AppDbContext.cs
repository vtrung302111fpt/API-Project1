using API_Project1.Entities;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    //tên bảng INVOICE_LIST đi với InvoiceListEntity
    public DbSet<InvoiceListEntity> INVOICE_LIST { get; set; } 

    public DbSet<GhiChuEntity> GHI_CHU { get; set; }

    public DbSet<InvoiceDetailEntity> INVOICE_DETAIL { get; set; }
    public DbSet<DsHangHoaEntity> DANH_SACH_HANG_HOA { get; set; }
    public DbSet<DsThueSuatEntity> DANH_SACH_THUE_SUAT { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InvoiceListEntity>()
            .HasOne(i => i.ghiChu)
            .WithOne(g => g.InvoiceListEntity)
            .HasForeignKey<GhiChuEntity>(g => g.ghiChu_id);

        base.OnModelCreating(modelBuilder);
    }
}
