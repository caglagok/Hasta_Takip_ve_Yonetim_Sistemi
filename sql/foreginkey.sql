ALTER TABLE [dbo].[Randevular]
ADD CONSTRAINT FK_Randevular_Doktorlar FOREIGN KEY (DoktorID) REFERENCES [dbo].[Doktorlar](DoktorID);
