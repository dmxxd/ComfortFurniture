
using System.Drawing;

namespace ComfortFurniture;

public sealed class MainForm : Form
{
    private readonly DataGridView grid = new();
    private readonly Label status = new();

    public MainForm()
    {
        Text = "Комфорт — управление продукцией";
        Width = 1180;
        Height = 720;
        MinimumSize = new Size(900, 550);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Candara", 11);
        BackColor = Color.White;

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 105,
            BackColor = ColorTranslator.FromHtml("#D2DFFF")
        };

        var logoPath = Path.Combine(
            AppContext.BaseDirectory,
            "Resources",
            "company.png"
        );

        if (File.Exists(logoPath))
        {
            using var image = Image.FromFile(logoPath);

            var logo = new PictureBox
            {
                Size = new Size(75, 75),
                Location = new Point(18, 15),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Image = new Bitmap(image)
            };

            header.Controls.Add(logo);
        }

        var title = new Label
        {
            Text = "Продукция компании «Комфорт»",
            AutoSize = true,
            Font = new Font("Candara", 22, FontStyle.Bold),
            ForeColor = ColorTranslator.FromHtml("#243B70"),
            Location = new Point(115, 32)
        };

        header.Controls.Add(title);

        var bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 64,
            Padding = new Padding(12, 10, 12, 10),
            BackColor = Color.White,
            WrapContents = false,
            AutoScroll = true
        };

        AddButton(bar, "Добавить", (_, _) => OpenEditor(null));
        AddButton(bar, "Редактировать", (_, _) => EditSelected());
        AddButton(bar, "Цеха продукта", (_, _) => ShowWorkshops());
        AddButton(bar, "Расчёт сырья", (_, _) =>
        {
            using var form = new RawMaterialForm();
            form.ShowDialog(this);
        });
        AddButton(bar, "Обновить", (_, _) => LoadProducts());

        grid.Dock = DockStyle.Fill;
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.None;
        grid.RowHeadersVisible = false;
        grid.EnableHeadersVisualStyles = false;

        grid.ColumnHeadersDefaultCellStyle.BackColor =
            ColorTranslator.FromHtml("#D2DFFF");
        grid.ColumnHeadersDefaultCellStyle.ForeColor =
            ColorTranslator.FromHtml("#243B70");
        grid.ColumnHeadersDefaultCellStyle.Font =
            new Font("Candara", 11, FontStyle.Bold);

        grid.ColumnHeadersHeight = 42;
        grid.RowTemplate.Height = 34;
        grid.DefaultCellStyle.Font = new Font("Candara", 11);
        grid.DefaultCellStyle.SelectionBackColor =
            ColorTranslator.FromHtml("#355CBD");
        grid.DefaultCellStyle.SelectionForeColor = Color.White;
        grid.AlternatingRowsDefaultCellStyle.BackColor =
            ColorTranslator.FromHtml("#F3F6FC");

        grid.CellDoubleClick += (_, _) => EditSelected();

        status.Dock = DockStyle.Bottom;
        status.Height = 30;
        status.Padding = new Padding(10, 5, 0, 0);
        status.Text = "Двойной щелчок по строке — редактирование";
        status.BackColor = ColorTranslator.FromHtml("#F3F6FC");
        status.ForeColor = ColorTranslator.FromHtml("#355CBD");

        Controls.Add(grid);
        Controls.Add(status);
        Controls.Add(bar);
        Controls.Add(header);

        Load += (_, _) => LoadProducts();
    }

    private static void AddButton(
        FlowLayoutPanel panel,
        string caption,
        EventHandler click)
    {
        int buttonWidth = caption switch
        {
            "Редактировать" => 205,
            "Цеха продукта" => 125,
            _ => 145
        };

        var button = new Button
        {
            Text = caption,
            Size = new Size(buttonWidth, 42),
            MinimumSize = new Size(buttonWidth, 42),
            MaximumSize = new Size(buttonWidth, 42),
            AutoSize = false,
            AutoEllipsis = false,
            TextAlign = ContentAlignment.MiddleCenter,
            UseCompatibleTextRendering = false,
            Margin = new Padding(5, 0, 5, 0),
            Padding = new Padding(0),
            BackColor = ColorTranslator.FromHtml("#355CBD"),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Candara", 9.5f),
            Cursor = Cursors.Hand
        };

        button.FlatAppearance.BorderSize = 0;
        button.Click += click;
        panel.Controls.Add(button);
    }

    private void LoadProducts()
    {
        try
        {
            grid.DataSource = Db.Query(
                "SELECT product_id AS ID, article AS Артикул, " +
                "product_type AS `Тип продукции`, name AS Наименование, " +
                "min_partner_price AS `Минимальная стоимость`, " +
                "main_material AS `Основной материал`, " +
                "total_hours AS `Время изготовления, ч` " +
                "FROM v_product_list ORDER BY name"
            );

            if (grid.Columns["ID"] != null)
            {
                grid.Columns["ID"].Visible = false;
            }

            status.Text = $"Загружено записей: {grid.Rows.Count}";
        }
        catch (Exception ex)
        {
            status.Text = "Не удалось загрузить продукцию";

            MessageBox.Show(
                "Не удалось загрузить продукцию.\n" +
                "Проверьте подключение к MySQL и запуск database.sql.\n\n" +
                ex.Message,
                "Ошибка базы данных",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    private int? SelectedId =>
        grid.CurrentRow == null
            ? null
            : Convert.ToInt32(grid.CurrentRow.Cells["ID"].Value);

    private void EditSelected()
    {
        if (SelectedId is int id)
        {
            OpenEditor(id);
        }
        else
        {
            MessageBox.Show(
                "Сначала выберите продукт.",
                "Внимание",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }
    }

    private void OpenEditor(int? id)
    {
        using var form = new ProductForm(id);

        if (form.ShowDialog(this) == DialogResult.OK)
        {
            LoadProducts();
        }
    }

    private void ShowWorkshops()
    {
        if (SelectedId is not int id)
        {
            MessageBox.Show(
                "Выберите продукт.",
                "Внимание",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            return;
        }

        try
        {
            var data = Db.Query(
                "SELECT w.name AS `Цех`, " +
                "w.workers_count AS `Количество работников`, " +
                "pw.production_hours AS `Время, ч` " +
                "FROM product_workshops pw " +
                "JOIN workshops w ON w.workshop_id = pw.workshop_id " +
                "WHERE pw.product_id = @id ORDER BY w.name",
                Db.P("@id", id)
            );

            if (data.Rows.Count == 0)
            {
                MessageBox.Show(
                    "Для выбранного продукта пока не указаны цеха.",
                    "Информация",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            using var form = new Form
            {
                Text = "Цеха изготовления продукта",
                Width = 720,
                Height = 450,
                MinimumSize = new Size(550, 350),
                StartPosition = FormStartPosition.CenterParent,
                Font = Font,
                BackColor = Color.White
            };

            var view = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                DataSource = data
            };

            view.ColumnHeadersDefaultCellStyle.BackColor =
                ColorTranslator.FromHtml("#D2DFFF");
            view.ColumnHeadersDefaultCellStyle.ForeColor =
                ColorTranslator.FromHtml("#243B70");
            view.ColumnHeadersDefaultCellStyle.Font =
                new Font("Candara", 11, FontStyle.Bold);

            view.ColumnHeadersHeight = 42;
            view.RowTemplate.Height = 34;
            view.DefaultCellStyle.Font = new Font("Candara", 11);
            view.DefaultCellStyle.SelectionBackColor =
                ColorTranslator.FromHtml("#355CBD");
            view.DefaultCellStyle.SelectionForeColor = Color.White;
            view.AlternatingRowsDefaultCellStyle.BackColor =
                ColorTranslator.FromHtml("#F3F6FC");

            form.Controls.Add(view);
            form.ShowDialog(this);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Ошибка получения списка цехов: " + ex.Message,
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
}
