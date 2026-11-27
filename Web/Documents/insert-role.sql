IF COL_LENGTH('dbo.TaiKhoan', 'YeuCauDoiMatKhau') IS NULL
BEGIN
    ALTER TABLE dbo.TaiKhoan
    ADD YeuCauDoiMatKhau BIT NOT NULL
        CONSTRAINT DF_TaiKhoan_YeuCauDoiMatKhau DEFAULT 1;
END
GO


SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    /* ==========================================
       1. KHOI TAO CAC VAI TRO
       ========================================== */

    DECLARE @VaiTro TABLE
    (
        MaVaiTro VARCHAR(50),
        TenVaiTro NVARCHAR(100),
        MoTa NVARCHAR(500)
    );

    INSERT INTO @VaiTro VALUES
    ('CHU_TRO', N'Chủ trọ', N'Quản trị toàn bộ hệ thống phòng trọ'),
    ('NHAN_VIEN', N'Nhân viên', N'Nhân viên vận hành với quyền cơ bản'),
    ('CU_DAN', N'Cư dân', N'Người thuê phòng trọ'),
    ('QUAN_LY', N'Quản lý', N'Quản lý hoạt động khu trọ'),
    ('KE_TOAN', N'Kế toán', N'Quản lý hóa đơn và thu chi'),
    ('KY_THUAT', N'Kỹ thuật', N'Tiếp nhận và xử lý yêu cầu sửa chữa');

    INSERT INTO dbo.VaiTro (MaVaiTro, TenVaiTro, MoTa)
    SELECT v.MaVaiTro, v.TenVaiTro, v.MoTa
    FROM @VaiTro v
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.VaiTro x
        WHERE x.MaVaiTro = v.MaVaiTro
    );

    /* ==========================================
       2. KHOI TAO DANH SACH QUYEN
       ========================================== */

    DECLARE @Quyen TABLE
    (
        MaQuyen VARCHAR(100),
        TenQuyen NVARCHAR(150),
        NhomQuyen VARCHAR(100),
        MoTa NVARCHAR(500)
    );

    INSERT INTO @Quyen VALUES

    -- TAI KHOAN VA PHAN QUYEN
    ('TAIKHOAN_VIEW', N'Xem tài khoản', 'TAIKHOAN', N'Xem danh sách tài khoản'),
    ('TAIKHOAN_CREATE', N'Tạo tài khoản', 'TAIKHOAN', N'Tạo tài khoản nhân viên và cư dân'),
    ('TAIKHOAN_UPDATE', N'Cập nhật tài khoản', 'TAIKHOAN', N'Cập nhật thông tin tài khoản'),
    ('TAIKHOAN_LOCK', N'Khóa tài khoản', 'TAIKHOAN', N'Khóa hoặc mở khóa tài khoản'),
    ('VAITRO_VIEW', N'Xem vai trò', 'PHANQUYEN', N'Xem danh sách vai trò'),
    ('VAITRO_MANAGE', N'Quản lý vai trò', 'PHANQUYEN', N'Tạo và cập nhật vai trò'),
    ('QUYEN_MANAGE', N'Quản lý quyền', 'PHANQUYEN', N'Quản lý danh sách quyền và gán quyền'),

    -- KHU TRO VA PHONG
    ('KHUTRO_VIEW', N'Xem khu trọ', 'KHUTRO', N'Xem thông tin khu trọ'),
    ('KHUTRO_MANAGE', N'Quản lý khu trọ', 'KHUTRO', N'Tạo, sửa, xóa khu trọ'),
    ('PHONG_VIEW', N'Xem phòng', 'PHONG', N'Xem danh sách phòng'),
    ('PHONG_CREATE', N'Tạo phòng', 'PHONG', N'Tạo phòng mới'),
    ('PHONG_UPDATE', N'Cập nhật phòng', 'PHONG', N'Cập nhật thông tin phòng'),
    ('PHONG_DELETE', N'Xóa phòng', 'PHONG', N'Xóa phòng'),

    -- CU DAN
    ('CUDAN_VIEW', N'Xem cư dân', 'CUDAN', N'Xem thông tin cư dân'),
    ('CUDAN_CREATE', N'Tạo cư dân', 'CUDAN', N'Tạo hồ sơ cư dân'),
    ('CUDAN_UPDATE', N'Cập nhật cư dân', 'CUDAN', N'Cập nhật hồ sơ cư dân'),
    ('CUDAN_DELETE', N'Xóa cư dân', 'CUDAN', N'Xóa hồ sơ cư dân'),

    -- HOP DONG
    ('HOPDONG_VIEW', N'Xem hợp đồng', 'HOPDONG', N'Xem hợp đồng'),
    ('HOPDONG_CREATE', N'Tạo hợp đồng', 'HOPDONG', N'Tạo hợp đồng thuê phòng'),
    ('HOPDONG_UPDATE', N'Cập nhật hợp đồng', 'HOPDONG', N'Cập nhật hợp đồng'),
    ('HOPDONG_CANCEL', N'Hủy hợp đồng', 'HOPDONG', N'Hủy hợp đồng thuê phòng'),

    -- DICH VU VA DIEN NUOC
    ('DICHVU_VIEW', N'Xem dịch vụ', 'DICHVU', N'Xem danh sách dịch vụ'),
    ('DICHVU_MANAGE', N'Quản lý dịch vụ', 'DICHVU', N'Tạo và cập nhật dịch vụ'),
    ('CHISO_VIEW', N'Xem chỉ số', 'CHISO', N'Xem chỉ số điện nước'),
    ('CHISO_CREATE', N'Nhập chỉ số', 'CHISO', N'Nhập chỉ số điện nước'),
    ('CHISO_UPDATE', N'Sửa chỉ số', 'CHISO', N'Điều chỉnh chỉ số điện nước'),

    -- HOA DON
    ('HOADON_VIEW', N'Xem hóa đơn', 'HOADON', N'Xem hóa đơn'),
    ('HOADON_CREATE', N'Tạo hóa đơn', 'HOADON', N'Lập hóa đơn'),
    ('HOADON_UPDATE', N'Cập nhật hóa đơn', 'HOADON', N'Cập nhật hóa đơn'),
    ('HOADON_CANCEL', N'Hủy hóa đơn', 'HOADON', N'Hủy hóa đơn'),
    ('HOADON_PAY', N'Thanh toán hóa đơn', 'HOADON', N'Thực hiện thanh toán hóa đơn'),

    -- THU CHI
    ('THUCHI_VIEW', N'Xem thu chi', 'THUCHI', N'Xem giao dịch thu chi'),
    ('THUCHI_CREATE', N'Tạo giao dịch thu chi', 'THUCHI', N'Tạo giao dịch thu chi'),
    ('THUCHI_UPDATE', N'Cập nhật thu chi', 'THUCHI', N'Cập nhật giao dịch'),
    ('THUCHI_DELETE', N'Xóa giao dịch thu chi', 'THUCHI', N'Xóa giao dịch'),

    -- SUA CHUA
    ('SUACHUA_VIEW', N'Xem yêu cầu sửa chữa', 'SUACHUA', N'Xem yêu cầu sửa chữa'),
    ('SUACHUA_CREATE', N'Tạo yêu cầu sửa chữa', 'SUACHUA', N'Tạo yêu cầu sửa chữa'),
    ('SUACHUA_ASSIGN', N'Phân công sửa chữa', 'SUACHUA', N'Phân công nhân viên xử lý'),
    ('SUACHUA_UPDATE', N'Cập nhật sửa chữa', 'SUACHUA', N'Cập nhật tiến độ sửa chữa'),

    -- KHACH DEN THAM
    ('KHACH_VIEW', N'Xem khách đến thăm', 'KHACH', N'Xem thông tin khách đến thăm'),
    ('KHACH_CREATE', N'Đăng ký khách', 'KHACH', N'Đăng ký khách đến thăm'),
    ('KHACH_MANAGE', N'Quản lý khách', 'KHACH', N'Quản lý lịch sử khách đến thăm'),

    -- THONG BAO
    ('THONGBAO_VIEW', N'Xem thông báo', 'THONGBAO', N'Xem thông báo'),
    ('THONGBAO_CREATE', N'Tạo thông báo', 'THONGBAO', N'Tạo thông báo'),
    ('THONGBAO_SEND', N'Gửi thông báo', 'THONGBAO', N'Gửi thông báo đến người dùng'),

    -- BAO CAO
    ('BAOCAO_VIEW', N'Xem báo cáo', 'BAOCAO', N'Xem báo cáo tổng hợp'),
    ('BAOCAO_TAICHINH', N'Xem báo cáo tài chính', 'BAOCAO', N'Xem báo cáo tài chính'),

    -- CA NHAN
    ('CANHAN_VIEW', N'Xem thông tin cá nhân', 'CANHAN', N'Xem hồ sơ cá nhân'),
    ('CANHAN_UPDATE', N'Cập nhật thông tin cá nhân', 'CANHAN', N'Cập nhật hồ sơ cá nhân'),

    -- HOP DONG / PHONG CUA CHINH MINH
    ('HOPDONG_SELF_VIEW', N'Xem hợp đồng của mình', 'CANHAN', N'Chỉ xem hợp đồng mình tham gia'),
    ('PHONG_SELF_VIEW', N'Xem phòng đang thuê', 'CANHAN', N'Chỉ xem phòng mình đang thuê'),
    ('HOADON_SELF_VIEW', N'Xem hóa đơn của mình', 'CANHAN', N'Chỉ xem hóa đơn của mình'),
    ('CHISO_SELF_VIEW', N'Xem chỉ số của mình', 'CANHAN', N'Chỉ xem chỉ số phòng mình thuê'),
    ('SUACHUA_SELF_VIEW', N'Xem yêu cầu sửa chữa của mình', 'CANHAN', N'Chỉ xem yêu cầu do mình tạo'),
    ('SUACHUA_SELF_CREATE', N'Tạo yêu cầu sửa chữa cá nhân', 'CANHAN', N'Tạo yêu cầu sửa chữa cho phòng mình thuê'),
    ('THONGBAO_SELF_VIEW', N'Xem thông báo của mình', 'CANHAN', N'Chỉ xem thông báo được gửi đến mình'),
    ('THANHTOAN_SELF', N'Thanh toán hóa đơn của mình', 'CANHAN', N'Thanh toán hóa đơn thuộc phòng mình thuê'),
    ('KHACH_SELF_CREATE', N'Đăng ký khách của mình', 'CANHAN', N'Đăng ký khách đến thăm phòng mình thuê');

    INSERT INTO dbo.Quyen (MaQuyen, TenQuyen, NhomQuyen, MoTa)
    SELECT q.MaQuyen, q.TenQuyen, q.NhomQuyen, q.MoTa
    FROM @Quyen q
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Quyen x
        WHERE x.MaQuyen = q.MaQuyen
    );

    /* ==========================================
       3. GAN QUYEN CHO CAC VAI TRO
       ========================================== */

    DECLARE @GanQuyen TABLE
    (
        MaVaiTro VARCHAR(50),
        MaQuyen VARCHAR(100)
    );

    -- NHAN VIEN: QUYEN VAN HANH CO BAN
    INSERT INTO @GanQuyen VALUES
    ('NHAN_VIEN', 'KHUTRO_VIEW'),
    ('NHAN_VIEN', 'PHONG_VIEW'),
    ('NHAN_VIEN', 'PHONG_UPDATE'),
    ('NHAN_VIEN', 'CUDAN_VIEW'),
    ('NHAN_VIEN', 'CUDAN_UPDATE'),
    ('NHAN_VIEN', 'HOPDONG_VIEW'),
    ('NHAN_VIEN', 'DICHVU_VIEW'),
    ('NHAN_VIEN', 'CHISO_VIEW'),
    ('NHAN_VIEN', 'CHISO_CREATE'),
    ('NHAN_VIEN', 'HOADON_VIEW'),
    ('NHAN_VIEN', 'SUACHUA_VIEW'),
    ('NHAN_VIEN', 'SUACHUA_UPDATE'),
    ('NHAN_VIEN', 'KHACH_VIEW'),
    ('NHAN_VIEN', 'KHACH_MANAGE'),
    ('NHAN_VIEN', 'THONGBAO_VIEW'),
    ('NHAN_VIEN', 'THONGBAO_CREATE'),
    ('NHAN_VIEN', 'CANHAN_VIEW'),
    ('NHAN_VIEN', 'CANHAN_UPDATE'),

    -- CU DAN: QUYEN CA NHAN
    ('CU_DAN', 'CANHAN_VIEW'),
    ('CU_DAN', 'CANHAN_UPDATE'),
    ('CU_DAN', 'PHONG_SELF_VIEW'),
    ('CU_DAN', 'HOPDONG_SELF_VIEW'),
    ('CU_DAN', 'HOADON_SELF_VIEW'),
    ('CU_DAN', 'CHISO_SELF_VIEW'),
    ('CU_DAN', 'SUACHUA_SELF_VIEW'),
    ('CU_DAN', 'SUACHUA_SELF_CREATE'),
    ('CU_DAN', 'THONGBAO_SELF_VIEW'),
    ('CU_DAN', 'THANHTOAN_SELF'),
    ('CU_DAN', 'KHACH_SELF_CREATE'),

    -- QUAN LY: QUAN LY HOAT DONG KHU TRO
    ('QUAN_LY', 'KHUTRO_VIEW'),
    ('QUAN_LY', 'PHONG_VIEW'),
    ('QUAN_LY', 'PHONG_CREATE'),
    ('QUAN_LY', 'PHONG_UPDATE'),
    ('QUAN_LY', 'CUDAN_VIEW'),
    ('QUAN_LY', 'CUDAN_CREATE'),
    ('QUAN_LY', 'CUDAN_UPDATE'),
    ('QUAN_LY', 'HOPDONG_VIEW'),
    ('QUAN_LY', 'HOPDONG_CREATE'),
    ('QUAN_LY', 'HOPDONG_UPDATE'),
    ('QUAN_LY', 'DICHVU_VIEW'),
    ('QUAN_LY', 'CHISO_VIEW'),
    ('QUAN_LY', 'CHISO_CREATE'),
    ('QUAN_LY', 'CHISO_UPDATE'),
    ('QUAN_LY', 'HOADON_VIEW'),
    ('QUAN_LY', 'HOADON_CREATE'),
    ('QUAN_LY', 'SUACHUA_VIEW'),
    ('QUAN_LY', 'SUACHUA_ASSIGN'),
    ('QUAN_LY', 'SUACHUA_UPDATE'),
    ('QUAN_LY', 'KHACH_VIEW'),
    ('QUAN_LY', 'KHACH_MANAGE'),
    ('QUAN_LY', 'THONGBAO_VIEW'),
    ('QUAN_LY', 'THONGBAO_CREATE'),
    ('QUAN_LY', 'THONGBAO_SEND'),
    ('QUAN_LY', 'BAOCAO_VIEW'),

    -- KE TOAN: HOA DON VA TAI CHINH
    ('KE_TOAN', 'KHUTRO_VIEW'),
    ('KE_TOAN', 'PHONG_VIEW'),
    ('KE_TOAN', 'CUDAN_VIEW'),
    ('KE_TOAN', 'HOPDONG_VIEW'),
    ('KE_TOAN', 'DICHVU_VIEW'),
    ('KE_TOAN', 'CHISO_VIEW'),
    ('KE_TOAN', 'HOADON_VIEW'),
    ('KE_TOAN', 'HOADON_CREATE'),
    ('KE_TOAN', 'HOADON_UPDATE'),
    ('KE_TOAN', 'HOADON_CANCEL'),
    ('KE_TOAN', 'THUCHI_VIEW'),
    ('KE_TOAN', 'THUCHI_CREATE'),
    ('KE_TOAN', 'THUCHI_UPDATE'),
    ('KE_TOAN', 'BAOCAO_VIEW'),
    ('KE_TOAN', 'BAOCAO_TAICHINH'),
    ('KE_TOAN', 'CANHAN_VIEW'),
    ('KE_TOAN', 'CANHAN_UPDATE'),

    -- KY THUAT: SUA CHUA VA THIET BI
    ('KY_THUAT', 'KHUTRO_VIEW'),
    ('KY_THUAT', 'PHONG_VIEW'),
    ('KY_THUAT', 'SUACHUA_VIEW'),
    ('KY_THUAT', 'SUACHUA_UPDATE'),
    ('KY_THUAT', 'CANHAN_VIEW'),
    ('KY_THUAT', 'CANHAN_UPDATE');

    /* ==========================================
       4. CHU TRO DUOC GAN TAT CA QUYEN
       ========================================== */

    INSERT INTO dbo.VaiTroQuyen (VaiTroId, QuyenId)
    SELECT vt.Id, q.Id
    FROM dbo.VaiTro vt
    CROSS JOIN dbo.Quyen q
    WHERE vt.MaVaiTro = 'CHU_TRO'
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.VaiTroQuyen x
          WHERE x.VaiTroId = vt.Id
            AND x.QuyenId = q.Id
      );

    /* ==========================================
       5. GAN QUYEN CHO CAC VAI TRO CON LAI
       ========================================== */

    INSERT INTO dbo.VaiTroQuyen (VaiTroId, QuyenId)
    SELECT vt.Id, q.Id
    FROM @GanQuyen g
    INNER JOIN dbo.VaiTro vt
        ON vt.MaVaiTro = g.MaVaiTro
    INNER JOIN dbo.Quyen q
        ON q.MaQuyen = g.MaQuyen
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.VaiTroQuyen x
        WHERE x.VaiTroId = vt.Id
          AND x.QuyenId = q.Id
    );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
