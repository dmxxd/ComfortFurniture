
using System.Drawing;
using System.Globalization;

namespace ComfortFurniture;

public sealed class ProductForm : Form
{
    private readonly int? productId;

    private readonly TextBox article = new();
    private readonly TextBox name = new();
    private readonly TextBox price = new();

    private readonly ComboBox type = new();
    private readonly ComboBox material = new();

    private int nextRow;

    public ProductForm(int? id)
    {
        productId = id;

        Text = id.HasValue
            ? "Редактирование продукции"
            : "Добавление продукции";

        Width = 780;
        Height = 540;
        MinimumSize = new Size(740, 500);
        StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Candara", 12);
        BackColor = Color.White;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(30),
            ColumnCount = 2,
            RowCount = 6,
            BackColor = Color.White
        };

        layout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 40));

        layout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 60));

        AddField(layout, "Артикул:", article);
        AddField(layout, "Тип продукта:", type);
        AddField(layout, "Наименование:", name);
        AddField(layout, "Минимальная стоимость:", price);
        AddField(layout, "Основной материал:", material);

        type.DropDownStyle = ComboBoxStyle.DropDownList;
        material.DropDownStyle = ComboBoxStyle.DropDownList;

        var save = new Button
        {
            Text = "Сохранить",
            Width = 200,
            Height = 48,
            MinimumSize = new Size(200, 48),
            MaximumSize = new Size(200, 48),
            AutoSize = false,
            AutoEllipsis = false,
            TextAlign = ContentAlignment.MiddleCenter,
            UseCompatibleTextRendering = false,
            Font = new Font("Candara", 11),
            BackColor = ColorTranslator.FromHtml("#355CBD"),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(5)
        };

        save.FlatAppearance.BorderSize = 0;
        save.Click += (_, _) => SaveProduct();

        var cancel = new Button
        {
            Text = "Отмена",
            Width = 150,
            Height = 48,
            MinimumSize = new Size(150, 48),
            MaximumSize = new Size(150, 48),
            AutoSize = false,
            AutoEllipsis = false,
            TextAlign = ContentAlignment.MiddleCenter,
            UseCompatibleTextRendering = false,
            Font = new Font("Candara", 11),
            BackColor = ColorTranslator.FromHtml("#E8EDFA"),
            ForeColor = ColorTranslator.FromHtml("#243B70"),
            FlatStyle = FlatStyle.Flat,
            DialogResult = DialogResult.Cancel,
            Margin = new Padding(5)
        };

        cancel.FlatAppearance.BorderSize = 0;

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            AutoScroll = false,
            Padding = new Padding(0, 8, 0, 0),
            BackColor = Color.White
        };

        buttons.Controls.Add(save);
        buttons.Controls.Add(cancel);

        for (int i = 0; i < 5; i++)
        {
            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 60));
        }

        layout.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(buttons, 0, 5);
        layout.SetColumnSpan(buttons, 2);

        Controls.Add(layout);

        AcceptButton = save;
        CancelButton = cancel;

        Load += (_, _) => LoadData();
    }

    private void AddField(
        TableLayoutPanel panel,
        string caption,
        Control control)
    {
        int row = nextRow++;

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
        control.Margin = new Padding(5, 8, 5, 8);
        control.Font = new Font("Candara", 11);

        panel.Controls.Add(label, 0, row);
        panel.Controls.Add(control, 1, row);
    }

    private void LoadData()
    {
        try
        {
            type.DataSource = Db.Query(
                "SELECT product_type_id, name " +
                "FROM product_types ORDER BY name");

            type.DisplayMember = "name";
            type.ValueMember = "product_type_id";

            material.DataSource = Db.Query(
                "SELECT material_type_id, name " +
                "FROM material_types ORDER BY name");

            material.DisplayMember = "name";
            material.ValueMember = "material_type_id";

            if (type.Items.Count == 0 || material.Items.Count == 0)
            {
                throw new InvalidOperationException(
                    "В базе данных отсутствуют типы продукции или материалы.");
            }

            if (productId is int id)
            {
                var dt = Db.Query(
                    "SELECT article, product_type_id, name, " +
                    "min_partner_price, material_type_id " +
                    "FROM products WHERE product_id = @id",
                    Db.P("@id", id));

                if (dt.Rows.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Продукт не найден.");
                }

                var r = dt.Rows[0];

                article.Text = r["article"].ToString();
                name.Text = r["name"].ToString();

                price.Text = Convert.ToDecimal(
                    r["min_partner_price"]
                ).ToString(CultureInfo.CurrentCulture);

                type.SelectedValue = Convert.ToInt32(
                    r["product_type_id"]);

                material.SelectedValue = Convert.ToInt32(
                    r["material_type_id"]);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Не удалось загрузить данные формы:\n" + ex.Message,
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

    private void SaveProduct()
    {
        string articleValue = article.Text.Trim();
        string nameValue = name.Text.Trim();

        if (string.IsNullOrWhiteSpace(articleValue) ||
            string.IsNullOrWhiteSpace(nameValue) ||
            type.SelectedValue == null ||
            material.SelectedValue == null)
        {
            MessageBox.Show(
                "Заполните все поля формы.",
                "Проверка данных",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        bool validPrice =
            decimal.TryParse(
                price.Text,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out decimal amount)
            ||
            decimal.TryParse(
                price.Text,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out amount);

        if (!validPrice ||
            amount < 0 ||
            decimal.Round(amount, 2) != amount)
        {
            MessageBox.Show(
                "Стоимость должна быть неотрицательным числом " +
                "с точностью не более двух знаков после запятой.",
                "Некорректная стоимость",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            price.Focus();
            return;
        }

        try
        {
            if (productId is int id)
            {
                Db.Execute(
                    "UPDATE products SET " +
                    "article = @article, " +
                    "product_type_id = @type, " +
                    "name = @name, " +
                    "min_partner_price = @price, " +
                    "material_type_id = @material " +
                    "WHERE product_id = @id",

                    Db.P("@article", articleValue),
                    Db.P("@type", type.SelectedValue),
                    Db.P("@name", nameValue),
                    Db.P("@price", amount),
                    Db.P("@material", material.SelectedValue),
                    Db.P("@id", id));
            }
            else
            {
                Db.Execute(
                    "INSERT INTO products " +
                    "(article, product_type_id, name, " +
                    "min_partner_price, material_type_id) " +
                    "VALUES (@article, @type, @name, @price, @material)",

                    Db.P("@article", articleValue),
                    Db.P("@type", type.SelectedValue),
                    Db.P("@name", nameValue),
                    Db.P("@price", amount),
                    Db.P("@material", material.SelectedValue));
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Не удалось сохранить продукт.\n" +
                "Проверьте данные и доступность базы.\n\n" +
                ex.Message,
                "Ошибка сохранения",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
