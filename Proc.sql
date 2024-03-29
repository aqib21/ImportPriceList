USE [BabtainData]
GO
/****** Object:  StoredProcedure [dbo].[SP_UpdatePriceList]    Script Date: 07-Jun-21 4:43:35 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   PROCEDURE [dbo].[SP_UpdatePriceList]
	@ItemCode NVARCHAR(100), 
	@UOM NVARCHAR(50), 
	@Price DECIMAL(19,9),
    @Message NVARCHAR(MAX) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @PackID INT,
	@PriceWithoutTax DECIMAL(19,9);

	SET @PackID = (SELECT P.PackID FROM Item I 
	INNER JOIN Pack P on P.ItemID = I.ItemID
	INNER JOIN PackTypeLanguage PTL ON PTL.PackTypeID = P.PackTypeID AND PTL.LanguageID = 1
	WHERE I.ItemCode = @ItemCode AND PTL.Description = @UOM);

	IF (@PackID IS NOT NULL) 
		BEGIN
			SET @PriceWithoutTax = (SELECT @Price / (1 + Tax/100) FROM PriceDefinition WHERE PackID = @PackID);

			UPDATE PriceDefinition SET Price = @PriceWithoutTax, MinPrice = @PriceWithoutTax, MaxPrice = @PriceWithoutTax 
			WHERE PackID = @PackID AND PriceListID = (SELECT KeyValue FROM Configuration WHERE ConfigurationID = 39);
			
			SET @Message = 'SUCCESS: Price has been updated for Item ' + @ItemCode + ' with UOM ' + @UOM;
		END
	ELSE
		SET @Message = 'ERROR: Item ' + @ItemCode + ' does not exist with UOM ' + @UOM;

	SELECT @Message
END
