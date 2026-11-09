
using System.Drawing;

namespace ComfortFurniture;

public sealed class RawMaterialForm : Form
{
    private readonly ComboBox productType = new();
    private readonly ComboBox materialType = new();

    private readonly NumericUpDown quantity = new()
    {
        Minimum = 1,
        Maximum = 1000000,
        Value = 1
    };

    private readonly NumericUpDown first = new()
    {
        Minimum = 0.0001M,
        Maximum = 100000,
        DecimalPlaces = 4,
        Increment = 0.1M,
        Value = 1
    };

    private readonly NumericUpDown second = new()
    {
        Minimum = 0.0001M,
        Maximum = 100000,
        DecimalPlaces = 4,
        Increment = 0.1M,
        Value = 1
    };

    private readonly Button calculate = new()
    {
        Text = "Рассчитать",
        Dock = DockStyle.Fill,
        Height = 55,
        MinimumSize = new Size(220, 55),
        BackColor = ColorTranslator.FromHtml("#355CBD"),
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        Font = new Font("Candara", 12),
        TextAlign = ContentAlignment.MiddleCenter,
        AutoSize = false,
        AutoEllipsis = false,
        UseCompatibleTextRendering = false,
        Margin = new Padding(5, 15, 5, 5),
        Cursor = Cursors.Hand
    };

    public RawMaterialForm()
    {
        Text = "Расчёт количества сырья";
        Width = 780;
        Height = 580;
        MinimumSize = new Size(720, 540);
        StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Candara", 12);
        BackColor = Color.White;

        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(35),
            ColumnCount = 2,
            RowCount = 6,
            BackColor = Color.White
        };

        panel.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 42));

        panel.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 58));

        productType.DropDownStyle = ComboBoxStyle.DropDownList;
        materialType.DropDownStyle = ComboBoxStyle.DropDownList;

        Add(panel, "Тип продукции:", productType);
        Add(panel, "Тип материала:", materialType);
        Add(panel, "Количество продукции:", quantity);
        Add(panel, "Параметр продукции 1:", first);
        Add(panel, "Параметр продукции 2:", second);

        calculate.FlatAppearance.BorderSize = 0;
        calculate.Click += (_, _) => Calculate();

        for (int i = 0; i < 5; i++)
        {
            panel.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 65));
        }

        panel.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100));

        panel.Controls.Add(calculate, 0, 5);
        panel.SetColumnSpan(calculate, 2);

        Controls.Add(panel);

        Load += (_, _) => LoadDirectories();
    }

    private static void Add(
        TableLayoutPanel panel,
        string caption,
        Control control)
    {
        int row = panel.Controls.Count / 2;

        var label = new Label
        {
            Text = caption,
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Candara", 11),
            ForeColor = ColorTranslator.FromHtml("#243B70")
        };

        control.Dock = DockStyle.Fill;
        control.Margin = new Padding(5, 10, 5, 10);
        control.Font = new Font("Candara", 11);

        panel.Controls.Add(label, 0, row);
        panel.Controls.Add(control, 1, row);
    }

    private void LoadDirectories()
    {
        try
        {
            var productTypes = Db.Query(
                "SELECT product_type_id, name " +
                "FROM product_types ORDER BY name");

            var materialTypes = Db.Query(
                "SELECT material_type_id, name " +
                "FROM material_types ORDER BY name");

            if (productTypes.Rows.Count == 0 ||
                materialTypes.Rows.Count == 0)
            {
                throw new InvalidOperationException(
                    "В базе данных отсутствуют типы продукции или материалы.");
            }

            productType.DataSource = productTypes;
            productType.DisplayMember = "name";
            productType.ValueMember = "product_type_id";

            materialType.DataSource = materialTypes;
            materialType.DisplayMember = "name";
            materialType.ValueMember = "material_type_id";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Не удалось загрузить справочники:\n" + ex.Message,
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

    private void Calculate()
    {
        if (productType.SelectedValue == null ||
            materialType.SelectedValue == null)
        {
            MessageBox.Show(
                "Выберите тип продукции и тип материала.",
                "Проверка данных",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        try
        {
            calculate.Enabled = false;

            int result =
                RawMaterialCalculator.CalculateRawMaterial(
                    Convert.ToInt32(productType.SelectedValue),
                    Convert.ToInt32(materialType.SelectedValue),
                    (int)quantity.Value,
                    (double)first.Value,
                    (double)second.Value
                );

            if (result < 0)
            {
                MessageBox.Show(
                    "Невозможно выполнить расчёт. " +
                    "Проверьте входные данные.",
                    "Результат расчёта",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show(
                    $"Необходимое количество сырья: {result}",
                    "Результат расчёта",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Ошибка расчёта:\n" + ex.Message,
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            calculate.Enabled = true;
        }
    }
}
