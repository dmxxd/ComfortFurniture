namespace ComfortFurniture;

public static class RawMaterialCalculator
{
    public static int CalculateRawMaterial(
        int productTypeId,
        int materialTypeId,
        int quantity,
        double parameter1,
        double parameter2)
    {
        if (productTypeId <= 0 ||
            materialTypeId <= 0 ||
            quantity <= 0 ||
            !double.IsFinite(parameter1) ||
            !double.IsFinite(parameter2) ||
            parameter1 <= 0 ||
            parameter2 <= 0)
        {
            return -1;
        }

        try
        {
            var factors = Db.Query(
                "SELECT pt.coefficient, mt.loss_percent " +
                "FROM product_types pt " +
                "CROSS JOIN material_types mt " +
                "WHERE pt.product_type_id = @productTypeId " +
                "AND mt.material_type_id = @materialTypeId",
                Db.P("@productTypeId", productTypeId),
                Db.P("@materialTypeId", materialTypeId)
            );

            if (factors.Rows.Count == 0)
            {
                return -1;
            }

            decimal coefficient = Convert.ToDecimal(
                factors.Rows[0]["coefficient"]);

            decimal lossPercent = Convert.ToDecimal(
                factors.Rows[0]["loss_percent"]);

            decimal firstParameter = Convert.ToDecimal(parameter1);
            decimal secondParameter = Convert.ToDecimal(parameter2);

            if (coefficient <= 0 ||
                lossPercent < 0 ||
                lossPercent >= 1)
            {
                return -1;
            }

            decimal required =
                quantity *
                firstParameter *
                secondParameter *
                coefficient /
                (1 - lossPercent);

            if (required <= 0 || required > int.MaxValue)
            {
                return -1;
            }

            return (int)Math.Ceiling(required);
        }
        catch
        {
            return -1;
        }
    }
}