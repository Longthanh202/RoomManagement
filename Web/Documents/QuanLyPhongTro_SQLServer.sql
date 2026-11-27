/*
===============================================================================
DATABASE: QuanLyPhongTro
HE QUAN TRI: Microsoft SQL Server
QUY UOC:
- Ten database, bang, cot, constraint: tieng Viet khong dau.
- Khoa chinh: BIGINT IDENTITY(1,1).
- Tien: DECIMAL(18,2); ngay gio: DATETIME2; chuoi Unicode: NVARCHAR.
- Cac bang tai chinh va hop dong khong nen xoa cung sau khi da phat sinh.
- Chay script tren SQL Server voi quyen tao database.
===============================================================================
*/

IF DB_ID(N'QuanLyPhongTro') IS NULL
    CREATE DATABASE QuanLyPhongTro;
GO
USE QuanLyPhongTro;
GO

/* ============================================================================
   NHOM 1 - CHU TRO, KHU TRO, DAY TRO, TANG, PHONG
   ============================================================================ */

-- ChuTro: Luu thong tin chu so huu/nguoi quan ly he thong nha tro.
CREATE TABLE dbo.ChuTro (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY, -- Ma dinh danh chu tro
    HoTen NVARCHAR(200) NOT NULL,                 -- Ho ten chu tro
    SoDienThoai VARCHAR(20) NULL,                 -- So dien thoai lien he
    Email VARCHAR(150) NULL,                      -- Email lien he
    DiaChi NVARCHAR(500) NULL,                    -- Dia chi cua chu tro
    TrangThai BIT NOT NULL CONSTRAINT DF_ChuTro_TrangThai DEFAULT 1, -- 1: hoat dong
    NgayTao DATETIME2 NOT NULL CONSTRAINT DF_ChuTro_NgayTao DEFAULT SYSDATETIME()
);
GO

-- KhuTro: Moi chu tro co the quan ly nhieu khu nha tro.
CREATE TABLE dbo.KhuTro (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY, -- Ma khu tro
    ChuTroId BIGINT NOT NULL,                     -- Chu tro so huu/quan ly
    MaKhuTro VARCHAR(50) NOT NULL,                -- Ma khu tro, duy nhat trong mot chu tro
    TenKhuTro NVARCHAR(200) NOT NULL,             -- Ten hien thi cua khu tro
    DiaChi NVARCHAR(500) NULL,                    -- Dia chi khu tro
    MoTa NVARCHAR(MAX) NULL,                      -- Mo ta, ghi chu
    TrangThai BIT NOT NULL CONSTRAINT DF_KhuTro_TrangThai DEFAULT 1,
    NgayTao DATETIME2 NOT NULL CONSTRAINT DF_KhuTro_NgayTao DEFAULT SYSDATETIME(),
    CONSTRAINT FK_KhuTro_ChuTro FOREIGN KEY (ChuTroId) REFERENCES dbo.ChuTro(Id),
    CONSTRAINT UQ_KhuTro_ChuTro_Ma UNIQUE (ChuTroId, MaKhuTro)
);
GO

-- DayTro: Cac day/toa nha thuoc mot khu tro.
CREATE TABLE dbo.DayTro (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    KhuTroId BIGINT NOT NULL,                     -- Khu tro chua day nay
    MaDayTro VARCHAR(50) NOT NULL,                -- Ma day, duy nhat trong khu tro
    TenDayTro NVARCHAR(200) NOT NULL,             -- Ten day (A, B, Day 1...)
    MoTa NVARCHAR(500) NULL,                      -- Mo ta day tro
    TrangThai BIT NOT NULL CONSTRAINT DF_DayTro_TrangThai DEFAULT 1,
    CONSTRAINT FK_DayTro_KhuTro FOREIGN KEY (KhuTroId) REFERENCES dbo.KhuTro(Id),
    CONSTRAINT UQ_DayTro_KhuTro_Ma UNIQUE (KhuTroId, MaDayTro)
);
GO

-- Tang: Danh sach tang cua tung day tro. Nha mot tang van co the tao tang 1.
CREATE TABLE dbo.Tang (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    DayTroId BIGINT NOT NULL,                     -- Day tro chua tang
    SoTang INT NOT NULL,                          -- So thu tu tang
    TenTang NVARCHAR(100) NULL,                   -- Ten tang de hien thi
    CONSTRAINT FK_Tang_DayTro FOREIGN KEY (DayTroId) REFERENCES dbo.DayTro(Id),
    CONSTRAINT UQ_Tang_DayTro_SoTang UNIQUE (DayTroId, SoTang)
);
GO

-- Phong: Thong tin vat ly va gia niem yet hien tai cua phong.
-- GiaThue chi la gia tham khao; gia thue da thoa thuan phai luu trong HopDong.
CREATE TABLE dbo.Phong (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TangId BIGINT NOT NULL,                       -- Tang chua phong
    MaPhong VARCHAR(50) NOT NULL,                 -- Ma phong trong mot tang
    TenPhong NVARCHAR(100) NULL,                  -- Ten goi phong
    DienTich DECIMAL(10,2) NULL,                  -- Dien tich m2
    SoNguoiToiDa INT NOT NULL CONSTRAINT DF_Phong_SoNguoi DEFAULT 4,
    GiaThue DECIMAL(18,2) NOT NULL CONSTRAINT DF_Phong_GiaThue DEFAULT 0, -- Gia niem yet
    TrangThai VARCHAR(30) NOT NULL CONSTRAINT DF_Phong_TrangThai DEFAULT 'Trong',
    MoTa NVARCHAR(MAX) NULL,                      -- Mo ta dac diem phong
    NgayTao DATETIME2 NOT NULL CONSTRAINT DF_Phong_NgayTao DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Phong_Tang FOREIGN KEY (TangId) REFERENCES dbo.Tang(Id),
    CONSTRAINT UQ_Phong_Tang_Ma UNIQUE (TangId, MaPhong),
    CONSTRAINT CK_Phong_Gia CHECK (GiaThue >= 0),
    CONSTRAINT CK_Phong_DienTich CHECK (DienTich IS NULL OR DienTich > 0),
    CONSTRAINT CK_Phong_SoNguoi CHECK (SoNguoiToiDa > 0),
    CONSTRAINT CK_Phong_TrangThai CHECK (TrangThai IN ('Trong','DangThue','DaDat','DangSuaChua','NgungHoatDong'))
);
GO

/* ============================================================================
   NHOM 2 - TAI KHOAN, VAI TRO, QUYEN, NHAN VIEN
   ============================================================================ */

-- TaiKhoan: Thong tin dang nhap. MatKhauHash phai la hash an toan, khong luu mat khau ro.
CREATE TABLE dbo.TaiKhoan (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenDangNhap VARCHAR(100) NOT NULL,            -- Ten dang nhap
    MatKhauHash VARCHAR(500) NOT NULL,            -- Chuoi hash mat khau
    Email VARCHAR(150) NULL,                      -- Email tai khoan
    SoDienThoai VARCHAR(20) NULL,                 -- So dien thoai tai khoan
    LoaiTaiKhoan VARCHAR(30) NOT NULL,            -- ChuTro/NhanVien/CuDan
    TrangThai BIT NOT NULL CONSTRAINT DF_TaiKhoan_TrangThai DEFAULT 1,
    LanDangNhapCuoi DATETIME2 NULL,               -- Lan dang nhap gan nhat
    NgayTao DATETIME2 NOT NULL CONSTRAINT DF_TaiKhoan_NgayTao DEFAULT SYSDATETIME(),
    CONSTRAINT UQ_TaiKhoan_TenDangNhap UNIQUE (TenDangNhap),
    CONSTRAINT CK_TaiKhoan_Loai CHECK (LoaiTaiKhoan IN ('ChuTro','NhanVien','CuDan'))
);
GO

-- VaiTro: Cac nhom vai tro nhu chu tro, quan ly, ke toan, ky thuat, cu dan.
CREATE TABLE dbo.VaiTro (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaVaiTro VARCHAR(50) NOT NULL,                -- Ma vai tro
    TenVaiTro NVARCHAR(100) NOT NULL,             -- Ten vai tro
    MoTa NVARCHAR(500) NULL,                      -- Mo ta vai tro
    TrangThai BIT NOT NULL CONSTRAINT DF_VaiTro_TrangThai DEFAULT 1,
    CONSTRAINT UQ_VaiTro_Ma UNIQUE (MaVaiTro)
);
GO

-- Quyen: Danh sach hanh dong he thong co the cap cho vai tro.
CREATE TABLE dbo.Quyen (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaQuyen VARCHAR(100) NOT NULL,                -- Ma quyen, vi du PHONG_VIEW
    TenQuyen NVARCHAR(150) NOT NULL,              -- Ten quyen hien thi
    NhomQuyen VARCHAR(100) NULL,                  -- Nhom module: PHONG, HOADON...
    MoTa NVARCHAR(500) NULL,                      -- Mo ta quyen
    CONSTRAINT UQ_Quyen_Ma UNIQUE (MaQuyen)
);
GO

-- TaiKhoanVaiTro: Bang trung gian quan he nhieu-nhieu tai khoan va vai tro.
CREATE TABLE dbo.TaiKhoanVaiTro (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TaiKhoanId BIGINT NOT NULL,                   -- Tai khoan duoc gan vai tro
    VaiTroId BIGINT NOT NULL,                     -- Vai tro duoc gan
    CONSTRAINT FK_TKVT_TaiKhoan FOREIGN KEY (TaiKhoanId) REFERENCES dbo.TaiKhoan(Id),
    CONSTRAINT FK_TKVT_VaiTro FOREIGN KEY (VaiTroId) REFERENCES dbo.VaiTro(Id),
    CONSTRAINT UQ_TKVT UNIQUE (TaiKhoanId, VaiTroId)
);
GO

-- VaiTroQuyen: Bang trung gian dinh nghia moi vai tro co nhung quyen nao.
CREATE TABLE dbo.VaiTroQuyen (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    VaiTroId BIGINT NOT NULL,                     -- Vai tro
    QuyenId BIGINT NOT NULL,                      -- Quyen thuoc vai tro
    CONSTRAINT FK_VTQ_VaiTro FOREIGN KEY (VaiTroId) REFERENCES dbo.VaiTro(Id),
    CONSTRAINT FK_VTQ_Quyen FOREIGN KEY (QuyenId) REFERENCES dbo.Quyen(Id),
    CONSTRAINT UQ_VTQ UNIQUE (VaiTroId, QuyenId)
);
GO

-- NhanVien: Ho so nhan su; TaiKhoanId lien ket tai khoan dang nhap neu co.
CREATE TABLE dbo.NhanVien (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TaiKhoanId BIGINT NOT NULL,                   -- Tai khoan cua nhan vien
    ChuTroId BIGINT NOT NULL,                     -- Chu tro quan ly nhan vien
    MaNhanVien VARCHAR(50) NOT NULL,              -- Ma nhan vien
    HoTen NVARCHAR(200) NOT NULL,                 -- Ho ten
    NgaySinh DATE NULL,                           -- Ngay sinh
    GioiTinh VARCHAR(20) NULL,                    -- Gioi tinh
    SoDienThoai VARCHAR(20) NULL,                 -- So dien thoai
    Email VARCHAR(150) NULL,                      -- Email
    DiaChi NVARCHAR(500) NULL,                    -- Dia chi
    ChucVu NVARCHAR(100) NULL,                    -- Chuc vu
    NgayVaoLam DATE NULL,                         -- Ngay bat dau lam
    NgayNghiViec DATE NULL,                       -- Ngay ket thuc lam
    TrangThai BIT NOT NULL CONSTRAINT DF_NhanVien_TrangThai DEFAULT 1,
    CONSTRAINT FK_NhanVien_TaiKhoan FOREIGN KEY (TaiKhoanId) REFERENCES dbo.TaiKhoan(Id),
    CONSTRAINT FK_NhanVien_ChuTro FOREIGN KEY (ChuTroId) REFERENCES dbo.ChuTro(Id),
    CONSTRAINT UQ_NhanVien_TaiKhoan UNIQUE (TaiKhoanId),
    CONSTRAINT UQ_NhanVien_Ma UNIQUE (ChuTroId, MaNhanVien)
);
GO

-- PhanCongNhanVien: Lich su phan cong nhan vien phu trach khu tro.
CREATE TABLE dbo.PhanCongNhanVien (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    NhanVienId BIGINT NOT NULL,                   -- Nhan vien duoc phan cong
    KhuTroId BIGINT NOT NULL,                     -- Khu tro phu trach
    NgayBatDau DATE NOT NULL,                     -- Ngay bat dau phu trach
    NgayKetThuc DATE NULL,                        -- Ngay ket thuc phu trach
    GhiChu NVARCHAR(500) NULL,                    -- Ghi chu phan cong
    CONSTRAINT FK_PCNV_NhanVien FOREIGN KEY (NhanVienId) REFERENCES dbo.NhanVien(Id),
    CONSTRAINT FK_PCNV_KhuTro FOREIGN KEY (KhuTroId) REFERENCES dbo.KhuTro(Id),
    CONSTRAINT CK_PCNV_Ngay CHECK (NgayKetThuc IS NULL OR NgayKetThuc >= NgayBatDau)
);
GO

/* ============================================================================
   NHOM 3 - CU DAN, HOP DONG, THANH VIEN, LICH SU CU TRU
   ============================================================================ */

-- CuDan: Ho so ca nhan cua nguoi thue/nguoi o cung. TaiKhoanId co the NULL neu chua dung app.
CREATE TABLE dbo.CuDan (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TaiKhoanId BIGINT NULL,                       -- Tai khoan dang nhap cua cu dan
    HoTen NVARCHAR(200) NOT NULL,                 -- Ho ten cu dan
    NgaySinh DATE NULL,                           -- Ngay sinh
    GioiTinh VARCHAR(20) NULL,                    -- Gioi tinh
    SoDienThoai VARCHAR(20) NULL,                 -- So dien thoai
    Email VARCHAR(150) NULL,                      -- Email
    SoCCCD VARCHAR(30) NULL,                      -- So can cuoc cong dan
    NgayCapCCCD DATE NULL,                        -- Ngay cap CCCD
    NoiCapCCCD NVARCHAR(200) NULL,                -- Noi cap CCCD
    DiaChiThuongTru NVARCHAR(500) NULL,           -- Dia chi thuong tru
    AnhDaiDien NVARCHAR(1000) NULL,               -- Duong dan anh dai dien
    TrangThai BIT NOT NULL CONSTRAINT DF_CuDan_TrangThai DEFAULT 1,
    NgayTao DATETIME2 NOT NULL CONSTRAINT DF_CuDan_NgayTao DEFAULT SYSDATETIME(),
    CONSTRAINT FK_CuDan_TaiKhoan FOREIGN KEY (TaiKhoanId) REFERENCES dbo.TaiKhoan(Id),
    CONSTRAINT UQ_CuDan_TaiKhoan UNIQUE (TaiKhoanId)
);
GO

-- HopDong: Thoa thuan thue phong. GiaThue/TienCoc la gia tri chot tai thoi diem ky.
CREATE TABLE dbo.HopDong (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaHopDong VARCHAR(50) NOT NULL,               -- Ma hop dong
    PhongId BIGINT NOT NULL,                      -- Phong duoc thue
    CuDanDaiDienId BIGINT NOT NULL,               -- Cu dan dai dien/nguoi thue chinh
    NgayBatDau DATE NOT NULL,                     -- Ngay bat dau hop dong
    NgayKetThuc DATE NOT NULL,                    -- Ngay ket thuc hop dong
    GiaThue DECIMAL(18,2) NOT NULL,               -- Tien thue moi ky (thong thuong moi thang)
    TienCoc DECIMAL(18,2) NOT NULL CONSTRAINT DF_HopDong_TienCoc DEFAULT 0, -- Tien dat coc
    NgayThanhToan INT NOT NULL CONSTRAINT DF_HopDong_NgayTT DEFAULT 5, -- Ngay den han hang thang
    SoThangThanhToan INT NOT NULL CONSTRAINT DF_HopDong_SoThang DEFAULT 1, -- Chu ky thu tien
    DieuKhoan NVARCHAR(MAX) NULL,                 -- Dieu khoan hop dong
    TrangThai VARCHAR(30) NOT NULL CONSTRAINT DF_HopDong_TrangThai DEFAULT 'ChoDuyet',
    NgayTao DATETIME2 NOT NULL CONSTRAINT DF_HopDong_NgayTao DEFAULT SYSDATETIME(),
    NguoiTao BIGINT NULL,                         -- Tai khoan tao hop dong
    CONSTRAINT FK_HopDong_Phong FOREIGN KEY (PhongId) REFERENCES dbo.Phong(Id),
    CONSTRAINT FK_HopDong_CuDan FOREIGN KEY (CuDanDaiDienId) REFERENCES dbo.CuDan(Id),
    CONSTRAINT FK_HopDong_NguoiTao FOREIGN KEY (NguoiTao) REFERENCES dbo.TaiKhoan(Id),
    CONSTRAINT UQ_HopDong_Ma UNIQUE (MaHopDong),
    CONSTRAINT CK_HopDong_Ngay CHECK (NgayKetThuc > NgayBatDau),
    CONSTRAINT CK_HopDong_Tien CHECK (GiaThue >= 0 AND TienCoc >= 0),
    CONSTRAINT CK_HopDong_NgayTT CHECK (NgayThanhToan BETWEEN 1 AND 31),
    CONSTRAINT CK_HopDong_SoThang CHECK (SoThangThanhToan > 0)
);
GO

-- HopDongThanhVien: Danh sach nguoi duoc phep cu tru theo tung hop dong.
CREATE TABLE dbo.HopDongThanhVien (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    HopDongId BIGINT NOT NULL,                    -- Hop dong
    CuDanId BIGINT NOT NULL,                      -- Cu dan la thanh vien
    LoaiThanhVien VARCHAR(30) NOT NULL,            -- NguoiThueChinh/ThanhVien/GiaDinh...
    LaNguoiDaiDien BIT NOT NULL CONSTRAINT DF_HDTV_DaiDien DEFAULT 0,
    NgayBatDau DATE NOT NULL,                     -- Ngay bat dau o theo hop dong
    NgayKetThuc DATE NULL,                        -- Ngay ket thuc o; NULL neu van dang o
    TrangThai BIT NOT NULL CONSTRAINT DF_HDTV_TrangThai DEFAULT 1,
    CONSTRAINT FK_HDTV_HopDong FOREIGN KEY (HopDongId) REFERENCES dbo.HopDong(Id),
    CONSTRAINT FK_HDTV_CuDan FOREIGN KEY (CuDanId) REFERENCES dbo.CuDan(Id),
    CONSTRAINT UQ_HDTV UNIQUE (HopDongId, CuDanId),
    CONSTRAINT CK_HDTV_Ngay CHECK (NgayKetThuc IS NULL OR NgayKetThuc >= NgayBatDau)
);
GO

-- LichSuCuTru: Luu lich su cu dan vao/ra tung phong qua cac hop dong.
CREATE TABLE dbo.LichSuCuTru (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CuDanId BIGINT NOT NULL,                      -- Cu dan
    PhongId BIGINT NOT NULL,                      -- Phong da cu tru
    HopDongId BIGINT NULL,                        -- Hop dong lien quan
    NgayVao DATE NOT NULL,                        -- Ngay vao o
    NgayRa DATE NULL,                             -- Ngay roi di; NULL neu chua roi
    LyDo NVARCHAR(500) NULL,                      -- Ly do chuyen/roi di
    CONSTRAINT FK_LSCT_CuDan FOREIGN KEY (CuDanId) REFERENCES dbo.CuDan(Id),
    CONSTRAINT FK_LSCT_Phong FOREIGN KEY (PhongId) REFERENCES dbo.Phong(Id),
    CONSTRAINT FK_LSCT_HopDong FOREIGN KEY (HopDongId) REFERENCES dbo.HopDong(Id),
    CONSTRAINT CK_LSCT_Ngay CHECK (NgayRa IS NULL OR NgayRa >= NgayVao)
);
GO

-- TaiLieuHopDong: Tep PDF/anh va cac phien ban tai lieu hop dong.
CREATE TABLE dbo.TaiLieuHopDong (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    HopDongId BIGINT NOT NULL,                    -- Hop dong so huu tai lieu
    TenFile NVARCHAR(255) NULL,                   -- Ten file goc
    DuongDanFile NVARCHAR(1000) NOT NULL,         -- URL/path luu file
    PhienBan INT NOT NULL CONSTRAINT DF_TLHD_PhienBan DEFAULT 1,
    NgayTaiLen DATETIME2 NOT NULL CONSTRAINT DF_TLHD_Ngay DEFAULT SYSDATETIME(),
    CONSTRAINT FK_TLHD_HopDong FOREIGN KEY (HopDongId) REFERENCES dbo.HopDong(Id)
);
GO

-- ChuKyHopDong: Thong tin tung nguoi ky, thoi diem ky va bang chung chu ky.
CREATE TABLE dbo.ChuKyHopDong (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    HopDongId BIGINT NOT NULL,                    -- Hop dong duoc ky
    CuDanId BIGINT NOT NULL,                      -- Nguoi ky
    LoaiNguoiKy VARCHAR(30) NULL,                 -- Dai dien chu tro/nguoi thue/thanh vien
    DuongDanChuKy NVARCHAR(1000) NULL,            -- Duong dan bang chung chu ky
    ThoiGianKy DATETIME2 NULL,                    -- Thoi diem ky
    TrangThai VARCHAR(30) NOT NULL CONSTRAINT DF_CKHD_TrangThai DEFAULT 'ChoKy',
    CONSTRAINT FK_CKHD_HopDong FOREIGN KEY (HopDongId) REFERENCES dbo.HopDong(Id),
    CONSTRAINT FK_CKHD_CuDan FOREIGN KEY (CuDanId) REFERENCES dbo.CuDan(Id)
);
GO

-- PhuLucHopDong: Cac thay doi dieu khoan, gia thue, thoi han sau khi ky hop dong.
CREATE TABLE dbo.PhuLucHopDong (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    HopDongId BIGINT NOT NULL,                    -- Hop dong goc
    MaPhuLuc VARCHAR(50) NOT NULL,                -- Ma phu luc
    LoaiPhuLuc VARCHAR(50) NULL,                  -- Tang gia/gia han/thay doi thanh vien...
    NoiDung NVARCHAR(MAX) NULL,                   -- Noi dung dieu chinh
    NgayHieuLuc DATE NULL,                        -- Ngay bat dau ap dung
    DuongDanFile NVARCHAR(1000) NULL,             -- File phu luc da ky
    NgayTao DATETIME2 NOT NULL CONSTRAINT DF_PLHD_Ngay DEFAULT SYSDATETIME(),
    CONSTRAINT FK_PLHD_HopDong FOREIGN KEY (HopDongId) REFERENCES dbo.HopDong(Id),
    CONSTRAINT UQ_PLHD_Ma UNIQUE (HopDongId, MaPhuLuc)
);
GO

/* ============================================================================
   NHOM 4 - DICH VU, BANG GIA, DONG HO, CHI SO, CHI PHI PHONG
   ============================================================================ */

-- DichVu: Danh muc cac khoan dich vu: dien, nuoc, internet, rac, gui xe...
CREATE TABLE dbo.DichVu (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaDichVu VARCHAR(50) NOT NULL,                -- Ma dich vu
    TenDichVu NVARCHAR(200) NOT NULL,             -- Ten dich vu
    LoaiTinhPhi VARCHAR(30) NOT NULL,             -- TheoChiSo/CoDinh/TheoNguoi/TheoXe...
    DonViTinh NVARCHAR(50) NULL,                  -- kWh, m3, phong, nguoi, xe...
    MoTa NVARCHAR(500) NULL,                      -- Mo ta cach tinh
    TrangThai BIT NOT NULL CONSTRAINT DF_DichVu_TrangThai DEFAULT 1,
    CONSTRAINT UQ_DichVu_Ma UNIQUE (MaDichVu),
    CONSTRAINT CK_DichVu_Loai CHECK (LoaiTinhPhi IN ('TheoChiSo','CoDinh','TheoNguoi','TheoXe','TheoSoLuong','TuNhap'))
);
GO

-- BangGiaDichVu: Lich su don gia theo khu tro va khoang thoi gian hieu luc.
CREATE TABLE dbo.BangGiaDichVu (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    DichVuId BIGINT NOT NULL,                     -- Dich vu duoc dinh gia
    KhuTroId BIGINT NOT NULL,                     -- Khu tro ap dung gia
    DonGia DECIMAL(18,2) NOT NULL,                -- Don gia
    TuNgay DATE NOT NULL,                         -- Ngay bat dau ap dung
    DenNgay DATE NULL,                            -- Ngay ket thuc; NULL neu con ap dung
    TrangThai BIT NOT NULL CONSTRAINT DF_BGDV_TrangThai DEFAULT 1,
    CONSTRAINT FK_BGDV_DichVu FOREIGN KEY (DichVuId) REFERENCES dbo.DichVu(Id),
    CONSTRAINT FK_BGDV_KhuTro FOREIGN KEY (KhuTroId) REFERENCES dbo.KhuTro(Id),
    CONSTRAINT CK_BGDV_DonGia CHECK (DonGia >= 0),
    CONSTRAINT CK_BGDV_Ngay CHECK (DenNgay IS NULL OR DenNgay >= TuNgay)
);
GO

-- DichVuPhong: Dich vu ap dung rieng cho phong, cho phep gia/cach tinh khac gia chung.
CREATE TABLE dbo.DichVuPhong (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    PhongId BIGINT NOT NULL,                      -- Phong ap dung
    DichVuId BIGINT NOT NULL,                     -- Dich vu
    DonGia DECIMAL(18,2) NOT NULL,                -- Don gia rieng cua phong
    NgayBatDau DATE NOT NULL,                     -- Ngay bat dau ap dung
    NgayKetThuc DATE NULL,                        -- Ngay ket thuc ap dung
    TrangThai BIT NOT NULL CONSTRAINT DF_DVP_TrangThai DEFAULT 1,
    CONSTRAINT FK_DVP_Phong FOREIGN KEY (PhongId) REFERENCES dbo.Phong(Id),
    CONSTRAINT FK_DVP_DichVu FOREIGN KEY (DichVuId) REFERENCES dbo.DichVu(Id),
    CONSTRAINT CK_DVP_DonGia CHECK (DonGia >= 0),
    CONSTRAINT CK_DVP_Ngay CHECK (NgayKetThuc IS NULL OR NgayKetThuc >= NgayBatDau)
);
GO

-- DongHo: Dong ho dien/nuoc gan tai phong; co the mo rong de quan ly dong ho tong.
CREATE TABLE dbo.DongHo (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    PhongId BIGINT NOT NULL,                      -- Phong gan dong ho
    DichVuId BIGINT NOT NULL,                     -- Dich vu do (dien/nuoc)
    MaDongHo VARCHAR(50) NOT NULL,                -- Ma quan ly dong ho
    SoSerial VARCHAR(100) NULL,                   -- So serial tren dong ho
    NgayLapDat DATE NULL,                         -- Ngay lap dat
    ChiSoBanDau DECIMAL(18,3) NOT NULL CONSTRAINT DF_DongHo_ChiSo DEFAULT 0,
    TrangThai BIT NOT NULL CONSTRAINT DF_DongHo_TrangThai DEFAULT 1,
    CONSTRAINT FK_DongHo_Phong FOREIGN KEY (PhongId) REFERENCES dbo.Phong(Id),
    CONSTRAINT FK_DongHo_DichVu FOREIGN KEY (DichVuId) REFERENCES dbo.DichVu(Id),
    CONSTRAINT UQ_DongHo_Ma UNIQUE (MaDongHo),
    CONSTRAINT CK_DongHo_ChiSo CHECK (ChiSoBanDau >= 0)
);
GO

-- ChiSoDongHo: Moi lan ghi dien/nuoc tao mot dong lich su, khong ghi de chi so cu.
CREATE TABLE dbo.ChiSoDongHo (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    DongHoId BIGINT NOT NULL,                     -- Dong ho duoc ghi
    NgayGhi DATETIME2 NOT NULL,                   -- Ngay gio ghi chi so
    ChiSoCu DECIMAL(18,3) NOT NULL,               -- Chi so dau ky
    ChiSoMoi DECIMAL(18,3) NOT NULL,              -- Chi so cuoi ky
    SoLuongTieuThu AS (ChiSoMoi - ChiSoCu) PERSISTED, -- San luong tieu thu tu dong
    AnhDongHo NVARCHAR(1000) NULL,                -- Anh chup dong ho
    NhanVienGhi BIGINT NULL,                      -- Nhan vien ghi chi so
    GhiChu NVARCHAR(500) NULL,                    -- Ghi chu
    CONSTRAINT FK_CSDH_DongHo FOREIGN KEY (DongHoId) REFERENCES dbo.DongHo(Id),
    CONSTRAINT FK_CSDH_NhanVien FOREIGN KEY (NhanVienGhi) REFERENCES dbo.NhanVien(Id),
    CONSTRAINT CK_CSDH_ChiSo CHECK (ChiSoCu >= 0 AND ChiSoMoi >= ChiSoCu)
);
GO

-- KyTinhTien: Chu ky chot chi phi (thang/ky tuy chinh) cua tung khu tro.
CREATE TABLE dbo.KyTinhTien (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    KhuTroId BIGINT NOT NULL,                     -- Khu tro tinh tien
    TenKy NVARCHAR(100) NOT NULL,                 -- Vi du: Thang 01/2026
    TuNgay DATE NOT NULL,                         -- Ngay dau ky
    DenNgay DATE NOT NULL,                        -- Ngay cuoi ky
    HanThanhToan DATE NULL,                       -- Han thanh toan du kien
    TrangThai VARCHAR(30) NOT NULL CONSTRAINT DF_KTT_TrangThai DEFAULT 'Moi',
    CONSTRAINT FK_KTT_KhuTro FOREIGN KEY (KhuTroId) REFERENCES dbo.KhuTro(Id),
    CONSTRAINT CK_KTT_Ngay CHECK (DenNgay >= TuNgay)
);
GO

-- ChiPhiPhong: Cac khoan phi da tinh cho tung phong trong mot ky.
CREATE TABLE dbo.ChiPhiPhong (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    KyTinhTienId BIGINT NOT NULL,                 -- Ky tinh tien
    PhongId BIGINT NOT NULL,                      -- Phong phat sinh chi phi
    DichVuId BIGINT NOT NULL,                     -- Dich vu tinh phi
    ChiSoDongHoId BIGINT NULL,                    -- Chi so dong ho neu tinh theo dong ho
    SoLuong DECIMAL(18,3) NOT NULL CONSTRAINT DF_CPP_SoLuong DEFAULT 1, -- Muc tieu thu/so luong
    DonGia DECIMAL(18,2) NOT NULL,                -- Don gia tai thoi diem tinh
    ThanhTien DECIMAL(18,2) NOT NULL,             -- So tien da chot
    GhiChu NVARCHAR(500) NULL,                    -- Ghi chu
    CONSTRAINT FK_CPP_Ky FOREIGN KEY (KyTinhTienId) REFERENCES dbo.KyTinhTien(Id),
    CONSTRAINT FK_CPP_Phong FOREIGN KEY (PhongId) REFERENCES dbo.Phong(Id),
    CONSTRAINT FK_CPP_DichVu FOREIGN KEY (DichVuId) REFERENCES dbo.DichVu(Id),
    CONSTRAINT FK_CPP_ChiSo FOREIGN KEY (ChiSoDongHoId) REFERENCES dbo.ChiSoDongHo(Id),
    CONSTRAINT UQ_CPP UNIQUE (KyTinhTienId, PhongId, DichVuId),
    CONSTRAINT CK_CPP_Tien CHECK (SoLuong >= 0 AND DonGia >= 0 AND ThanhTien >= 0)
);
GO

/* ============================================================================
   NHOM 5 - XE VA THIET BI
   ============================================================================ */

-- LoaiXe: Danh muc xe va muc phi gui mac dinh.
CREATE TABLE dbo.LoaiXe (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenLoaiXe NVARCHAR(100) NOT NULL,             -- Xe may, o to, xe dap...
    PhiGuiMacDinh DECIMAL(18,2) NOT NULL CONSTRAINT DF_LoaiXe_Phi DEFAULT 0,
    TrangThai BIT NOT NULL CONSTRAINT DF_LoaiXe_TrangThai DEFAULT 1,
    CONSTRAINT CK_LoaiXe_Phi CHECK (PhiGuiMacDinh >= 0)
);
GO

-- Xe: Phuong tien cua cu dan.
CREATE TABLE dbo.Xe (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CuDanId BIGINT NOT NULL,                      -- Chu so huu xe
    LoaiXeId BIGINT NOT NULL,                     -- Loai xe
    BienSo VARCHAR(30) NULL,                      -- Bien so xe
    HangXe NVARCHAR(100) NULL,                    -- Hang xe
    MauXe NVARCHAR(50) NULL,                      -- Mau xe
    NgayDangKy DATE NULL,                         -- Ngay dang ky gui
    TrangThai BIT NOT NULL CONSTRAINT DF_Xe_TrangThai DEFAULT 1,
    CONSTRAINT FK_Xe_CuDan FOREIGN KEY (CuDanId) REFERENCES dbo.CuDan(Id),
    CONSTRAINT FK_Xe_LoaiXe FOREIGN KEY (LoaiXeId) REFERENCES dbo.LoaiXe(Id)
);
GO

-- XePhong: Lich su xe dang gui tai phong nao va muc phi ap dung.
CREATE TABLE dbo.XePhong (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    XeId BIGINT NOT NULL,                         -- Xe duoc dang ky
    PhongId BIGINT NOT NULL,                      -- Phong gui xe
    NgayGui DATE NOT NULL,                        -- Ngay bat dau gui
    NgayNgungGui DATE NULL,                       -- Ngay ket thuc gui
    PhiGui DECIMAL(18,2) NOT NULL,                -- Phi gui xe chot cho lan dang ky
    TrangThai BIT NOT NULL CONSTRAINT DF_XePhong_TrangThai DEFAULT 1,
    CONSTRAINT FK_XP_Xe FOREIGN KEY (XeId) REFERENCES dbo.Xe(Id),
    CONSTRAINT FK_XP_Phong FOREIGN KEY (PhongId) REFERENCES dbo.Phong(Id),
    CONSTRAINT CK_XP_Ngay CHECK (NgayNgungGui IS NULL OR NgayNgungGui >= NgayGui),
    CONSTRAINT CK_XP_Phi CHECK (PhiGui >= 0)
);
GO

-- LoaiThietBi: Danh muc nhom thiet bi (dieu hoa, tu lanh, giuong...).
CREATE TABLE dbo.LoaiThietBi (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaLoai VARCHAR(50) NOT NULL,                  -- Ma loai thiet bi
    TenLoai NVARCHAR(200) NOT NULL,               -- Ten loai thiet bi
    MoTa NVARCHAR(500) NULL,                      -- Mo ta
    CONSTRAINT UQ_LoaiThietBi_Ma UNIQUE (MaLoai)
);
GO

-- ThietBiPhong: Tung tai san/thiet bi cu the dang duoc lap dat tai mot phong.
CREATE TABLE dbo.ThietBiPhong (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    PhongId BIGINT NOT NULL,                      -- Phong dang dat thiet bi
    LoaiThietBiId BIGINT NOT NULL,                -- Nhom thiet bi
    MaThietBi VARCHAR(50) NOT NULL,               -- Ma quan ly tai san
    TenThietBi NVARCHAR(200) NULL,                -- Ten cu the
    SoSerial VARCHAR(100) NULL,                  -- So serial
    NgayMua DATE NULL,                            -- Ngay mua
    GiaMua DECIMAL(18,2) NULL,                    -- Gia mua
    TinhTrang VARCHAR(30) NOT NULL CONSTRAINT DF_TBP_TinhTrang DEFAULT 'Tot', -- Tot/Hong/Mat...
    TrangThai BIT NOT NULL CONSTRAINT DF_TBP_TrangThai DEFAULT 1, -- Dang su dung
    CONSTRAINT FK_TBP_Phong FOREIGN KEY (PhongId) REFERENCES dbo.Phong(Id),
    CONSTRAINT FK_TBP_Loai FOREIGN KEY (LoaiThietBiId) REFERENCES dbo.LoaiThietBi(Id),
    CONSTRAINT UQ_TBP_Ma UNIQUE (MaThietBi),
    CONSTRAINT CK_TBP_Gia CHECK (GiaMua IS NULL OR GiaMua >= 0)
);
GO

-- BanGiaoThietBi: Ghi nhan hien trang thiet bi tai thoi diem giao/nhan phong.
CREATE TABLE dbo.BanGiaoThietBi (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ThietBiPhongId BIGINT NOT NULL,               -- Thiet bi duoc ban giao
    HopDongId BIGINT NOT NULL,                    -- Hop dong/lan ban giao lien quan
    NgayBanGiao DATE NOT NULL,                    -- Ngay ban giao
    TinhTrang NVARCHAR(200) NULL,                 -- Tinh trang tai thoi diem ban giao
    AnhBanGiao NVARCHAR(1000) NULL,               -- Anh minh chung
    GhiChu NVARCHAR(500) NULL,                    -- Ghi chu
    CONSTRAINT FK_BGTTB_ThietBi FOREIGN KEY (ThietBiPhongId) REFERENCES dbo.ThietBiPhong(Id),
    CONSTRAINT FK_BGTTB_HopDong FOREIGN KEY (HopDongId) REFERENCES dbo.HopDong(Id)
);
GO

/* ============================================================================
   NHOM 6 - SUA CHUA, KHACH DEN CHOI
   ============================================================================ */

-- YeuCauSuaChua: Yeu cau bao hong/bao tri do cu dan gui.
CREATE TABLE dbo.YeuCauSuaChua (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaYeuCau VARCHAR(50) NOT NULL,                -- Ma yeu cau
    PhongId BIGINT NOT NULL,                      -- Phong can sua
    CuDanId BIGINT NOT NULL,                      -- Cu dan gui yeu cau
    ThietBiPhongId BIGINT NULL,                   -- Thiet bi hong neu co
    TieuDe NVARCHAR(200) NOT NULL,                -- Tieu de su co
    NoiDung NVARCHAR(MAX) NULL,                   -- Mo ta chi tiet
    MucDoUuTien VARCHAR(30) NOT NULL CONSTRAINT DF_YCSC_UuTien DEFAULT 'BinhThuong',
    TrangThai VARCHAR(30) NOT NULL CONSTRAINT DF_YCSC_TrangThai DEFAULT 'ChoTiepNhan',
    NgayTao DATETIME2 NOT NULL CONSTRAINT DF_YCSC_NgayTao DEFAULT SYSDATETIME(),
    NgayHoanThanh DATETIME2 NULL,                 -- Thoi diem hoan tat
    CONSTRAINT FK_YCSC_Phong FOREIGN KEY (PhongId) REFERENCES dbo.Phong(Id),
    CONSTRAINT FK_YCSC_CuDan FOREIGN KEY (CuDanId) REFERENCES dbo.CuDan(Id),
    CONSTRAINT FK_YCSC_ThietBi FOREIGN KEY (ThietBiPhongId) REFERENCES dbo.ThietBiPhong(Id),
    CONSTRAINT UQ_YCSC_Ma UNIQUE (MaYeuCau)
);
GO

-- PhanCongSuaChua: Nhan vien duoc giao xu ly mot yeu cau.
CREATE TABLE dbo.PhanCongSuaChua (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    YeuCauSuaChuaId BIGINT NOT NULL,              -- Yeu cau duoc phan cong
    NhanVienId BIGINT NOT NULL,                   -- Nhan vien xu ly
    NgayPhanCong DATETIME2 NOT NULL CONSTRAINT DF_PCSC_Ngay DEFAULT SYSDATETIME(),
    GhiChu NVARCHAR(500) NULL,                    -- Noi dung phan cong
    CONSTRAINT FK_PCSC_YeuCau FOREIGN KEY (YeuCauSuaChuaId) REFERENCES dbo.YeuCauSuaChua(Id),
    CONSTRAINT FK_PCSC_NhanVien FOREIGN KEY (NhanVienId) REFERENCES dbo.NhanVien(Id)
);
GO

-- LichSuSuaChua: Luu tung lan thay doi trang thai/tien do xu ly yeu cau.
CREATE TABLE dbo.LichSuSuaChua (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    YeuCauSuaChuaId BIGINT NOT NULL,              -- Yeu cau lien quan
    NhanVienId BIGINT NULL,                       -- Nguoi cap nhat
    TrangThaiCu VARCHAR(30) NULL,                 -- Trang thai truoc cap nhat
    TrangThaiMoi VARCHAR(30) NULL,                -- Trang thai sau cap nhat
    NoiDung NVARCHAR(MAX) NULL,                   -- Ghi chu xu ly
    NgayCapNhat DATETIME2 NOT NULL CONSTRAINT DF_LSSC_Ngay DEFAULT SYSDATETIME(),
    CONSTRAINT FK_LSSC_YeuCau FOREIGN KEY (YeuCauSuaChuaId) REFERENCES dbo.YeuCauSuaChua(Id),
    CONSTRAINT FK_LSSC_NhanVien FOREIGN KEY (NhanVienId) REFERENCES dbo.NhanVien(Id)
);
GO

-- ChiPhiSuaChua: Vat tu, linh kien, cong tho... phat sinh cho yeu cau sua chua.
CREATE TABLE dbo.ChiPhiSuaChua (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    YeuCauSuaChuaId BIGINT NOT NULL,              -- Yeu cau phat sinh chi phi
    TenChiPhi NVARCHAR(200) NOT NULL,             -- Ten vat tu/dich vu
    SoLuong DECIMAL(18,2) NOT NULL CONSTRAINT DF_CPSC_SoLuong DEFAULT 1,
    DonGia DECIMAL(18,2) NOT NULL,                -- Don gia
    ThanhTien DECIMAL(18,2) NOT NULL,             -- Thanh tien
    NgayPhatSinh DATE NOT NULL,                   -- Ngay phat sinh
    CONSTRAINT FK_CPSC_YeuCau FOREIGN KEY (YeuCauSuaChuaId) REFERENCES dbo.YeuCauSuaChua(Id),
    CONSTRAINT CK_CPSC_Tien CHECK (SoLuong >= 0 AND DonGia >= 0 AND ThanhTien >= 0)
);
GO

-- DangKyKhach: Cu dan khai bao khach den tham phong.
CREATE TABLE dbo.DangKyKhach (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    PhongId BIGINT NOT NULL,                      -- Phong co khach den
    CuDanId BIGINT NOT NULL,                      -- Cu dan dang ky/chu nha tiep
    TenKhach NVARCHAR(200) NOT NULL,              -- Ho ten khach
    SoDienThoai VARCHAR(20) NULL,                -- Dien thoai khach
    SoCCCD VARCHAR(30) NULL,                      -- CCCD neu can ghi nhan
    SoLuongKhach INT NOT NULL CONSTRAINT DF_DKK_SoLuong DEFAULT 1,
    ThoiGianDuKienDen DATETIME2 NULL,             -- Gio du kien den
    ThoiGianDuKienVe DATETIME2 NULL,              -- Gio du kien ve
    GhiChu NVARCHAR(500) NULL,                    -- Ghi chu
    TrangThai VARCHAR(30) NOT NULL CONSTRAINT DF_DKK_TrangThai DEFAULT 'ChoDuyet',
    CONSTRAINT FK_DKK_Phong FOREIGN KEY (PhongId) REFERENCES dbo.Phong(Id),
    CONSTRAINT FK_DKK_CuDan FOREIGN KEY (CuDanId) REFERENCES dbo.CuDan(Id),
    CONSTRAINT CK_DKK_SoLuong CHECK (SoLuongKhach > 0),
    CONSTRAINT CK_DKK_ThoiGian CHECK (ThoiGianDuKienVe IS NULL OR ThoiGianDuKienDen IS NULL OR ThoiGianDuKienVe >= ThoiGianDuKienDen)
);
GO

-- LichSuKhach: Luu su kien khach check-in/check-out va nguoi ghi nhan.
CREATE TABLE dbo.LichSuKhach (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    DangKyKhachId BIGINT NOT NULL,                -- Dang ky khach lien quan
    LoaiSuKien VARCHAR(30) NOT NULL,              -- CheckIn/CheckOut/Huy
    ThoiGian DATETIME2 NOT NULL CONSTRAINT DF_LSK_ThoiGian DEFAULT SYSDATETIME(),
    NhanVienId BIGINT NULL,                       -- Nhan vien ghi nhan
    GhiChu NVARCHAR(500) NULL,                    -- Ghi chu
    CONSTRAINT FK_LSK_DKK FOREIGN KEY (DangKyKhachId) REFERENCES dbo.DangKyKhach(Id),
    CONSTRAINT FK_LSK_NhanVien FOREIGN KEY (NhanVienId) REFERENCES dbo.NhanVien(Id)
);
GO

/* ============================================================================
   NHOM 7 - HOA DON, CHI TIET, THANH TOAN, PHAN BO
   ============================================================================ */

-- HoaDon: Tong hop tien phong va cac dich vu cua mot phong trong mot ky.
CREATE TABLE dbo.HoaDon (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaHoaDon VARCHAR(50) NOT NULL,                -- Ma hoa don
    PhongId BIGINT NOT NULL,                      -- Phong phai thanh toan
    HopDongId BIGINT NOT NULL,                    -- Hop dong ap dung
    KyTinhTienId BIGINT NOT NULL,                 -- Ky tinh tien
    NgayLap DATE NOT NULL,                        -- Ngay phat hanh
    HanThanhToan DATE NOT NULL,                   -- Han cuoi thanh toan
    TongTien DECIMAL(18,2) NOT NULL CONSTRAINT DF_HoaDon_Tong DEFAULT 0, -- Tong truoc giam
    GiamGia DECIMAL(18,2) NOT NULL CONSTRAINT DF_HoaDon_Giam DEFAULT 0, -- So tien giam
    TongThanhToan DECIMAL(18,2) NOT NULL CONSTRAINT DF_HoaDon_PhaiTra DEFAULT 0, -- Tong phai tra
    DaThanhToan DECIMAL(18,2) NOT NULL CONSTRAINT DF_HoaDon_DaTra DEFAULT 0, -- Da thu
    ConNo AS (TongThanhToan - DaThanhToan) PERSISTED, -- Cong no con lai
    TrangThai VARCHAR(30) NOT NULL CONSTRAINT DF_HoaDon_TrangThai DEFAULT 'ChuaThanhToan',
    NgayTao DATETIME2 NOT NULL CONSTRAINT DF_HoaDon_NgayTao DEFAULT SYSDATETIME(),
    CONSTRAINT FK_HoaDon_Phong FOREIGN KEY (PhongId) REFERENCES dbo.Phong(Id),
    CONSTRAINT FK_HoaDon_HopDong FOREIGN KEY (HopDongId) REFERENCES dbo.HopDong(Id),
    CONSTRAINT FK_HoaDon_Ky FOREIGN KEY (KyTinhTienId) REFERENCES dbo.KyTinhTien(Id),
    CONSTRAINT UQ_HoaDon_Ma UNIQUE (MaHoaDon),
    CONSTRAINT UQ_HoaDon_Phong_Ky UNIQUE (PhongId, KyTinhTienId),
    CONSTRAINT CK_HoaDon_Tien CHECK (TongTien >= 0 AND GiamGia >= 0 AND TongThanhToan >= 0 AND DaThanhToan >= 0 AND DaThanhToan <= TongThanhToan)
);
GO

-- ChiTietHoaDon: Tung dong khoan thu; luu ten, so luong, don gia tai luc phat hanh.
CREATE TABLE dbo.ChiTietHoaDon (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    HoaDonId BIGINT NOT NULL,                     -- Hoa don cha
    DichVuId BIGINT NULL,                         -- Dich vu lien quan; NULL neu la khoan thu tu do
    TenKhoanThu NVARCHAR(200) NOT NULL,            -- Tien phong/dien/nuoc/internet...
    SoLuong DECIMAL(18,3) NOT NULL CONSTRAINT DF_CTHD_SoLuong DEFAULT 1,
    DonViTinh NVARCHAR(50) NULL,                  -- kWh, m3, thang, xe...
    DonGia DECIMAL(18,2) NOT NULL,                -- Don gia tai thoi diem lap hoa don
    ThanhTien DECIMAL(18,2) NOT NULL,             -- Thanh tien da tinh
    GhiChu NVARCHAR(500) NULL,                    -- Ghi chu
    CONSTRAINT FK_CTHD_HoaDon FOREIGN KEY (HoaDonId) REFERENCES dbo.HoaDon(Id),
    CONSTRAINT FK_CTHD_DichVu FOREIGN KEY (DichVuId) REFERENCES dbo.DichVu(Id),
    CONSTRAINT CK_CTHD_Tien CHECK (SoLuong >= 0 AND DonGia >= 0 AND ThanhTien >= 0)
);
GO

-- ThanhToan: Giao dich thu tien tu tien mat/chuyen khoan/QR/cong thanh toan.
CREATE TABLE dbo.ThanhToan (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaGiaoDich VARCHAR(100) NOT NULL,             -- Ma giao dich noi bo
    TaiKhoanId BIGINT NULL,                       -- Tai khoan ghi nhan giao dich
    SoTien DECIMAL(18,2) NOT NULL,                -- So tien giao dich
    PhuongThuc VARCHAR(30) NOT NULL,              -- TienMat/ChuyenKhoan/VietQR...
    MaGiaoDichNganHang VARCHAR(200) NULL,         -- Ma tham chieu tu ngan hang
    NgayThanhToan DATETIME2 NULL,                 -- Thoi diem thanh toan thanh cong
    TrangThai VARCHAR(30) NOT NULL CONSTRAINT DF_ThanhToan_TrangThai DEFAULT 'ChoXuLy',
    GhiChu NVARCHAR(500) NULL,                    -- Ghi chu giao dich
    NgayTao DATETIME2 NOT NULL CONSTRAINT DF_ThanhToan_NgayTao DEFAULT SYSDATETIME(),
    CONSTRAINT FK_ThanhToan_TaiKhoan FOREIGN KEY (TaiKhoanId) REFERENCES dbo.TaiKhoan(Id),
    CONSTRAINT UQ_ThanhToan_Ma UNIQUE (MaGiaoDich),
    CONSTRAINT CK_ThanhToan_SoTien CHECK (SoTien > 0)
);
GO

-- PhanBoThanhToan: Gan mot giao dich vao mot hoac nhieu hoa don.
CREATE TABLE dbo.PhanBoThanhToan (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ThanhToanId BIGINT NOT NULL,                  -- Giao dich thanh toan
    HoaDonId BIGINT NOT NULL,                     -- Hoa don duoc thanh toan
    SoTienPhanBo DECIMAL(18,2) NOT NULL,           -- So tien cua giao dich gan vao hoa don nay
    CONSTRAINT FK_PBTT_ThanhToan FOREIGN KEY (ThanhToanId) REFERENCES dbo.ThanhToan(Id),
    CONSTRAINT FK_PBTT_HoaDon FOREIGN KEY (HoaDonId) REFERENCES dbo.HoaDon(Id),
    CONSTRAINT UQ_PBTT UNIQUE (ThanhToanId, HoaDonId),
    CONSTRAINT CK_PBTT_SoTien CHECK (SoTienPhanBo > 0)
);
GO

-- DieuChinhHoaDon: Luu bu tru/giam tru/dieu chinh hoa don; khong sua lich su chi tiet cu.
CREATE TABLE dbo.DieuChinhHoaDon (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    HoaDonId BIGINT NOT NULL,                     -- Hoa don can dieu chinh
    LoaiDieuChinh VARCHAR(30) NOT NULL,            -- Tang/Giam/Huy...
    SoTien DECIMAL(18,2) NOT NULL,                -- Gia tri dieu chinh
    LyDo NVARCHAR(500) NULL,                      -- Ly do dieu chinh
    NguoiTao BIGINT NULL,                         -- Tai khoan tao dieu chinh
    NgayTao DATETIME2 NOT NULL CONSTRAINT DF_DCHD_Ngay DEFAULT SYSDATETIME(),
    CONSTRAINT FK_DCHD_HoaDon FOREIGN KEY (HoaDonId) REFERENCES dbo.HoaDon(Id),
    CONSTRAINT FK_DCHD_TaiKhoan FOREIGN KEY (NguoiTao) REFERENCES dbo.TaiKhoan(Id)
);
GO

/* ============================================================================
   NHOM 8 - THU CHI, THONG BAO, NHAT KY
   ============================================================================ */

-- DanhMucThuChi: Phan loai cac khoan thu va chi de lap bao cao.
CREATE TABLE dbo.DanhMucThuChi (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaDanhMuc VARCHAR(50) NOT NULL,               -- Ma danh muc
    TenDanhMuc NVARCHAR(200) NOT NULL,            -- Ten danh muc
    LoaiGiaoDich VARCHAR(20) NOT NULL,            -- Thu/Chi
    MoTa NVARCHAR(500) NULL,                      -- Mo ta
    TrangThai BIT NOT NULL CONSTRAINT DF_DMTC_TrangThai DEFAULT 1,
    CONSTRAINT UQ_DMTC_Ma UNIQUE (MaDanhMuc),
    CONSTRAINT CK_DMTC_Loai CHECK (LoaiGiaoDich IN ('Thu','Chi'))
);
GO

-- GiaoDichThuChi: So cai thu/chi cap khu tro, phuc vu bao cao dong tien.
-- Khong ghi nhan trung doanh thu: giao dich thu tu hoa don can duoc doi soat voi ThanhToan.
CREATE TABLE dbo.GiaoDichThuChi (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    KhuTroId BIGINT NOT NULL,                     -- Khu tro phat sinh thu/chi
    DanhMucId BIGINT NOT NULL,                    -- Danh muc thu/chi
    MaGiaoDich VARCHAR(50) NOT NULL,              -- Ma giao dich thu chi
    LoaiGiaoDich VARCHAR(20) NOT NULL,            -- Thu/Chi
    SoTien DECIMAL(18,2) NOT NULL,                -- So tien
    NgayGiaoDich DATETIME2 NOT NULL,              -- Ngay phat sinh
    NoiDung NVARCHAR(500) NULL,                   -- Dien giai
    NguoiTao BIGINT NULL,                         -- Tai khoan tao
    NgayTao DATETIME2 NOT NULL CONSTRAINT DF_GDTC_NgayTao DEFAULT SYSDATETIME(),
    CONSTRAINT FK_GDTC_KhuTro FOREIGN KEY (KhuTroId) REFERENCES dbo.KhuTro(Id),
    CONSTRAINT FK_GDTC_DanhMuc FOREIGN KEY (DanhMucId) REFERENCES dbo.DanhMucThuChi(Id),
    CONSTRAINT FK_GDTC_TaiKhoan FOREIGN KEY (NguoiTao) REFERENCES dbo.TaiKhoan(Id),
    CONSTRAINT UQ_GDTC_Ma UNIQUE (MaGiaoDich),
    CONSTRAINT CK_GDTC_SoTien CHECK (SoTien > 0),
    CONSTRAINT CK_GDTC_Loai CHECK (LoaiGiaoDich IN ('Thu','Chi'))
);
GO

-- ThongBao: Noi dung thong bao chung hoac thong bao theo doi tuong tham chieu.
CREATE TABLE dbo.ThongBao (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TieuDe NVARCHAR(200) NOT NULL,                -- Tieu de thong bao
    NoiDung NVARCHAR(MAX) NULL,                   -- Noi dung chi tiet
    LoaiThongBao VARCHAR(50) NULL,               -- HoaDon/SuaChua/Khach/HeThong...
    KhuTroId BIGINT NULL,                         -- NULL neu thong bao toan he thong
    NguoiTao BIGINT NULL,                         -- Tai khoan tao thong bao
    LoaiDoiTuongThamChieu VARCHAR(50) NULL,       -- Loai bang lien quan
    DoiTuongThamChieuId BIGINT NULL,              -- Id ban ghi lien quan
    NgayTao DATETIME2 NOT NULL CONSTRAINT DF_ThongBao_NgayTao DEFAULT SYSDATETIME(),
    CONSTRAINT FK_ThongBao_KhuTro FOREIGN KEY (KhuTroId) REFERENCES dbo.KhuTro(Id),
    CONSTRAINT FK_ThongBao_TaiKhoan FOREIGN KEY (NguoiTao) REFERENCES dbo.TaiKhoan(Id)
);
GO

-- NguoiNhanThongBao: Danh sach tai khoan nhan va trang thai da doc/chua doc.
CREATE TABLE dbo.NguoiNhanThongBao (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ThongBaoId BIGINT NOT NULL,                   -- Thong bao duoc nhan
    TaiKhoanId BIGINT NOT NULL,                   -- Tai khoan nhan
    DaDoc BIT NOT NULL CONSTRAINT DF_NNTB_DaDoc DEFAULT 0,
    ThoiGianDoc DATETIME2 NULL,                   -- Thoi diem doc
    CONSTRAINT FK_NNTB_ThongBao FOREIGN KEY (ThongBaoId) REFERENCES dbo.ThongBao(Id),
    CONSTRAINT FK_NNTB_TaiKhoan FOREIGN KEY (TaiKhoanId) REFERENCES dbo.TaiKhoan(Id),
    CONSTRAINT UQ_NNTB UNIQUE (ThongBaoId, TaiKhoanId)
);
GO

-- NhatKyHeThong: Truy vet thao tac them/sua/xoa va thay doi du lieu quan trong.
CREATE TABLE dbo.NhatKyHeThong (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TaiKhoanId BIGINT NULL,                       -- Tai khoan thuc hien
    TenBang VARCHAR(100) NULL,                    -- Bang bi tac dong
    IdBanGhi BIGINT NULL,                         -- Id ban ghi bi tac dong
    HanhDong VARCHAR(30) NULL,                    -- Them/Sua/Xoa/DangNhap...
    DuLieuCu NVARCHAR(MAX) NULL,                  -- Du lieu truoc khi thay doi (JSON)
    DuLieuMoi NVARCHAR(MAX) NULL,                 -- Du lieu sau khi thay doi (JSON)
    DiaChiIP VARCHAR(50) NULL,                    -- IP nguoi thao tac
    ThoiGian DATETIME2 NOT NULL CONSTRAINT DF_NKHT_ThoiGian DEFAULT SYSDATETIME(),
    CONSTRAINT FK_NKHT_TaiKhoan FOREIGN KEY (TaiKhoanId) REFERENCES dbo.TaiKhoan(Id)
);
GO

/* ============================================================================
   NHOM 9 - INDEX PHUC VU CAC TRUY VAN THUONG GAP
   ============================================================================ */

CREATE INDEX IX_KhuTro_ChuTroId ON dbo.KhuTro(ChuTroId);
CREATE INDEX IX_DayTro_KhuTroId ON dbo.DayTro(KhuTroId);
CREATE INDEX IX_Tang_DayTroId ON dbo.Tang(DayTroId);
CREATE INDEX IX_Phong_Tang_TrangThai ON dbo.Phong(TangId, TrangThai);
CREATE INDEX IX_CuDan_SoDienThoai ON dbo.CuDan(SoDienThoai);
CREATE INDEX IX_CuDan_SoCCCD ON dbo.CuDan(SoCCCD);
CREATE INDEX IX_HopDong_Phong_TrangThai ON dbo.HopDong(PhongId, TrangThai);
CREATE INDEX IX_HopDong_NgayKetThuc ON dbo.HopDong(NgayKetThuc);
CREATE INDEX IX_HDTV_CuDanId ON dbo.HopDongThanhVien(CuDanId);
CREATE INDEX IX_CSDH_DongHo_NgayGhi ON dbo.ChiSoDongHo(DongHoId, NgayGhi);
CREATE INDEX IX_HoaDon_PhongId ON dbo.HoaDon(PhongId);
CREATE INDEX IX_HoaDon_KyTinhTienId ON dbo.HoaDon(KyTinhTienId);
CREATE INDEX IX_HoaDon_TrangThai_HanThanhToan ON dbo.HoaDon(TrangThai, HanThanhToan);
CREATE INDEX IX_ThanhToan_NgayThanhToan ON dbo.ThanhToan(NgayThanhToan);
CREATE INDEX IX_YCSC_Phong_TrangThai ON dbo.YeuCauSuaChua(PhongId, TrangThai);
CREATE INDEX IX_NNTB_TaiKhoan_DaDoc ON dbo.NguoiNhanThongBao(TaiKhoanId, DaDoc);
GO

/* ============================================================================
   NHOM 10 - DU LIEU DANH MUC MAU
   ============================================================================ */

-- Vai tro mac dinh.
INSERT INTO dbo.VaiTro (MaVaiTro, TenVaiTro, MoTa) VALUES
('CHUTRO', N'Chu tro', N'Quan ly toan bo khu tro thuoc quyen so huu'),
('QUANLY', N'Quan ly khu tro', N'Quan ly hoat dong khu tro duoc phan cong'),
('KETOAN', N'Ke toan', N'Quan ly hoa don, thanh toan va thu chi'),
('KYTHUAT', N'Ky thuat', N'Tiep nhan va xu ly yeu cau sua chua'),
('LETAN', N'Le tan', N'Ho tro cu dan va quan ly khach ra vao'),
('CUDAN', N'Cu dan', N'Tra cuu hoa don va gui yeu cau dich vu');

-- Quyen mau.
INSERT INTO dbo.Quyen (MaQuyen, TenQuyen, NhomQuyen) VALUES
('PHONG_VIEW', N'Xem phong', 'PHONG'),
('PHONG_CREATE', N'Them phong', 'PHONG'),
('PHONG_UPDATE', N'Cap nhat phong', 'PHONG'),
('HOPDONG_VIEW', N'Xem hop dong', 'HOPDONG'),
('HOPDONG_CREATE', N'Tao hop dong', 'HOPDONG'),
('HOADON_VIEW', N'Xem hoa don', 'HOADON'),
('HOADON_CREATE', N'Tao hoa don', 'HOADON'),
('THANHTOAN_CREATE', N'Ghi nhan thanh toan', 'THANHTOAN'),
('BAOCAO_VIEW', N'Xem bao cao', 'BAOCAO'),
('NHANVIEN_MANAGE', N'Quan ly nhan vien', 'NHANVIEN');

-- Dich vu mau.
INSERT INTO dbo.DichVu (MaDichVu, TenDichVu, LoaiTinhPhi, DonViTinh) VALUES
('DIEN', N'Tien dien', 'TheoChiSo', N'kWh'),
('NUOC', N'Tien nuoc', 'TheoChiSo', N'm3'),
('INTERNET', N'Internet', 'CoDinh', N'Phong'),
('RAC', N'Phi rac', 'CoDinh', N'Phong'),
('VE_SINH', N'Phi ve sinh', 'CoDinh', N'Phong'),
('GUI_XE', N'Phi gui xe', 'TheoXe', N'Xe'),
('QUAN_LY', N'Phi quan ly', 'CoDinh', N'Phong');

-- Loai xe mau.
INSERT INTO dbo.LoaiXe (TenLoaiXe, PhiGuiMacDinh) VALUES
(N'Xe may', 100000), (N'Xe dap dien', 50000),
(N'O to', 1000000), (N'Xe dap', 30000);

-- Loai thiet bi mau.
INSERT INTO dbo.LoaiThietBi (MaLoai, TenLoai) VALUES
('DIEU_HOA', N'Dieu hoa'), ('TU_LANH', N'Tu lanh'),
('MAY_GIAT', N'May giat'), ('GIUONG', N'Giuong'),
('TU_QUAN_AO', N'Tu quan ao'), ('BINH_NONG_LANH', N'Binh nong lanh'),
('QUAT', N'Quat'), ('BAN_GHE', N'Ban ghe');

-- Danh muc thu chi mau.
INSERT INTO dbo.DanhMucThuChi (MaDanhMuc, TenDanhMuc, LoaiGiaoDich) VALUES
('THU_TIEN_PHONG', N'Thu tien phong', 'Thu'),
('THU_DIEN', N'Thu tien dien', 'Thu'),
('THU_NUOC', N'Thu tien nuoc', 'Thu'),
('THU_DICH_VU', N'Thu phi dich vu', 'Thu'),
('THU_TIEN_COC', N'Thu tien coc', 'Thu'),
('CHI_SUA_CHUA', N'Chi phi sua chua', 'Chi'),
('CHI_DIEN_CHUNG', N'Tien dien khu vuc chung', 'Chi'),
('CHI_NUOC_CHUNG', N'Tien nuoc khu vuc chung', 'Chi'),
('CHI_NHAN_VIEN', N'Luong nhan vien', 'Chi'),
('CHI_VE_SINH', N'Chi phi ve sinh', 'Chi');
GO

/* ============================================================================
   LUU Y NGHIEP VU QUAN TRONG
   1. Kiem tra hop dong bi trung khoang ngay cua cung mot phong trong transaction
      hoac stored procedure; UNIQUE thong thuong khong chan duoc khoang ngay giao nhau.
   2. Khi phat hanh hoa don, sao chep don gia/so luong vao ChiTietHoaDon de giu lich su.
   3. Khi thanh toan, dung transaction: ghi ThanhToan, PhanBoThanhToan va cap nhat
      HoaDon.DaThanhToan/TrangThai. Kiem tra tong phan bo khong vuot so tien giao dich
      va so tien con no cua hoa don.
   4. Khong cho phep client tu cap nhat truc tiep DaThanhToan.
   5. Bang DongHo trong ban nay gan voi Phong. Neu dung dong ho tong cho nhieu phong,
      can bo sung bang DongHoKhuTro/DongHoPhong hoac mo hinh phan bo chi phi rieng.
   6. Chu ky gia trong BangGiaDichVu khong duoc chong lan nhau cho cung DichVu/KhuTro;
      can kiem tra tai service/stored procedure.
   7. Tai lieu va anh chi luu duong dan (URL/path), khong nen luu binary truc tiep
      trong cac bang nghiep vu neu khong co yeu cau dac biet.
===============================================================================
*/
