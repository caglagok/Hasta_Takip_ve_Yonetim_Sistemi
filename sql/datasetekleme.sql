-- Eðer #TempTable adýnda bir tablo zaten varsa sil
IF OBJECT_ID('tempdb..#TempTable') IS NOT NULL
    DROP TABLE #TempTable;

-- JSON dosyasýný geçici bir tabloya aktarma
SELECT *
INTO #TempTable
FROM OPENROWSET (BULK 'C:\Users\CASPER\Desktop\Dataset\NewDataSet.json', SINGLE_CLOB) AS json;
-- JSON verisini parçalama ve Hastalar tablosuna ekleme
INSERT INTO Hastalar (Ad, Soyad, DogumTarihi, Cinsiyet, TelefonNumarasi, Adres, Password)
SELECT h.Ad, h.Soyad, h.DogumTarihi, h.Cinsiyet, h.TelefonNumarasi, h.Adres, h.Password
FROM OPENJSON((SELECT * FROM #TempTable), '$.Hastalar')
WITH (Ad NVARCHAR(50), Soyad NVARCHAR(50), DogumTarihi DATE, Cinsiyet NVARCHAR(10), TelefonNumarasi NVARCHAR(20), Adres NVARCHAR(100), Password NVARCHAR(50)
) AS h;
-- JSON verisini parçalama ve Doktorlar tablosuna ekleme
INSERT INTO Doktorlar (Ad, Soyad, UzmanlikAlani, CalistigiHastane, Password)
SELECT j.Ad, j.Soyad, j.UzmanlikAlani, j.CalistigiHastane, j.Password
FROM OPENJSON((SELECT * FROM #TempTable), '$.Doktorlar')
WITH (Ad NVARCHAR(50), Soyad NVARCHAR(50), UzmanlikAlani NVARCHAR(100), CalistigiHastane NVARCHAR(100), Password NVARCHAR(50)) AS j;

-- JSON verisini parçalama ve Yoneticiler tablosuna ekleme
INSERT INTO Yoneticiler (Ad, Soyad, Password)
SELECT y.Ad, y.Soyad, y.Password
FROM OPENJSON((SELECT * FROM #TempTable), '$.Yoneticiler')
WITH (Ad NVARCHAR(50), Soyad NVARCHAR(50), Password NVARCHAR(50)) AS y;

-- JSON verisini parçalama ve Randevu tablosuna ekleme
INSERT INTO Randevular (HastaID, DoktorID, RandevuTarihi, RandevuSaati)
SELECT r.HastaID, r.DoktorID, r.RandevuTarihi, r.RandevuSaati
FROM OPENJSON((SELECT * FROM #TempTable), '$.Randevular')
WITH (HastaID INT, DoktorID INT,RandevuTarihi DATE, RandevuSaati TIME(7)) AS r;
-- JSON verisini Parçalama ve TibbiRaporlar tablosuna ekleme
INSERT INTO TibbiRaporlar (HastaID, DoktorID,RaporTarihi, RaporIcerigi, URL)
SELECT t.HastaID, t.DoktorID,t.RaporTarihi, t.RaporIcerigi, t.URL
FROM OPENJSON((SELECT * FROM #TempTable), '$.TibbiRaporlar')
WITH (HastaID INT, DoktorID INT,RaporTarihi DATE, RaporIcerigi NVARCHAR(MAX), URL NVARCHAR(225)) AS t;


-- Geçici tabloyu temizleme
DROP TABLE #TempTable;