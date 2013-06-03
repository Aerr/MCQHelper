using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Text;
using System.Drawing;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        int currentPage, maxPage, maxQuestions;
        string[] questions;
        int[] answers;
        Label[] labels;
        CheckedListBox[] checkedList;
        string qPath;
        Legend initLegend;
        private int[,] totalAnswers;
        private string[] seriesArray;
        private int number;

        public Form1()
        {
            InitializeComponent();
            creationBox.BringToFront();
            initLegend = chart1.Legends[0];

            restart.Visible = false;

            toolTip.SetToolTip(saveChart, "Cliquer pour sauvegarder en image");
            saveChart.Visible = false;
            toolTip.SetToolTip(zoomChart, "Cliquer pour mettre en plein écran");
            zoomChart.Visible = false;
            toolTip.SetToolTip(showLegend, "Cliquer pour cacher/montrer la légende");
            showLegend.Visible = false;

            notes.Visible = false;
            notesBox.Visible = false;

            date.Text = "";
            currentPage = 1;
            labels = new Label[] { label1, label2, label3, label4, label5 };
            checkedList = new CheckedListBox[] { checkedListBox1, checkedListBox2, checkedListBox3, checkedListBox4, checkedListBox5 };
            for (int i = 0; i < 5; i++)
            {
                checkedList[i].Items.Clear();
                checkedList[i].Items.Add("Pas du tout d'accord");
                checkedList[i].Items.Add("Pas d'accord");
                checkedList[i].Items.Add("D'accord");
                checkedList[i].Items.Add("Tout à fait d'accord");
            }

            chart1.Visible = false;
        }

        #region Read/Write Config

        private void Form1_Shown(object sender, EventArgs e)
        {
            read_config();

            if (!creationBox.Visible)
            {
                read_Questions();

                checkEnabled();
            }
        }
        private void read_config()
        {
            try
            {
                using (StreamReader reader = new StreamReader("config.ini"))
                {
                    autoSave.Checked = Boolean.Parse(reader.ReadLine());
                    qPath = reader.ReadLine();
                }
                bool IsExists = System.IO.Directory.Exists(String.Format("FICHIERS_{0}/", qPath.Substring(0, qPath.Length - 4), nameBox.Text));

                if (!IsExists)
                    System.IO.Directory.CreateDirectory(String.Format("FICHIERS_{0}/", qPath.Substring(0, qPath.Length - 4), nameBox.Text));
            }
            catch
            {
                DialogResult result = MessageBox.Show("Aucun fichier de configuration n'a été trouvé, l'éditeur de questionnaire va donc être ouvert. Afin que le fichier soit correct, vous devez entrer une question par ligne (pour chacune, appuyez sur 'Entrée' une fois fini). Lorsque vous avez terminé, entrez un nom, puis sauvegardez.",
                   "Aucun fichier de configuration n'a été trouvé !",
                   MessageBoxButtons.OKCancel,
                   MessageBoxIcon.Asterisk,
                   MessageBoxDefaultButton.Button2);
                if (result == System.Windows.Forms.DialogResult.OK)
                    CreateMCQ_Click(null, null);
                else
                    quit_Click(null, null);
            }
        }
        private void write_config()
        {
            using (StreamWriter writer = new StreamWriter("config.ini"))
            {
                writer.WriteLine(autoSave.Checked);
                writer.WriteLine(qPath);
            }
        }
        private void read_Questions()
        {
            try
            {
                using (StreamReader sr = new StreamReader(qPath))
                {
                    questions = new string[500];
                    int i = 0;
                    while (!sr.EndOfStream)
                    {
                        questions[i] = sr.ReadLine();
                        if (questions[i] != "")
                            i++;
                    }
                    maxQuestions = i;
                }
                if (maxQuestions % 5 == 0)
                    maxPage = maxQuestions / 5;
                else
                    maxPage = (int)(maxQuestions / 5) + 1;
                answers = new int[maxQuestions];
                for (int i = 0; i < maxQuestions; i++)
                {
                    answers[i] = -1;
                }
                newQuestions();
                nameBox.Select();
            }
            catch
            {
                DialogResult result = MessageBox.Show("Le fichier contenant les questions est introuvable : choisissez-en un nouveau.",
                   "Fichier question introuvable !",
                   MessageBoxButtons.OKCancel,
                   MessageBoxIcon.Asterisk,
                   MessageBoxDefaultButton.Button2);
                if (result == System.Windows.Forms.DialogResult.OK)
                    changeMCQ_Click(null, null);
                else
                    quit_Click(null, null);
            }
        }
        #endregion

        #region Page changing
        private bool allChecked(bool validate)
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (checkedList[i].GetItemChecked(j))
                        answers[(((currentPage - 1) * 5) + i)] = j;
                }
            }

            bool good = true;
            int k = 0;
            foreach (int answer in answers)
            {
                if (!validate && (++k > 5 * currentPage))
                    break;
                else if (answer < 0)
                {
                    good = false;
                    break;
                }
            }

            if (!good)
            {
                MessageBox.Show("Vous n'avez pas répondu à toutes les questions !",
                    "Vous n'avez pas répondu à toutes les questions !",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button1);
            }
            return good;
        }

        private void next_Click(object sender, EventArgs e)
        {
            if (allChecked(false))
            {
                currentPage++;
                newQuestions();
                checkEnabled();
            }
        }

        private void previous_Click(object sender, EventArgs e)
        {
            currentPage--;
            newQuestions();
            checkEnabled();
        }

        private void newQuestions()
        {
            for (int i = 0; i < 5; i++)
            {
                labels[i].Font = new System.Drawing.Font("Tahoma", 11.25f);
                if (((currentPage - 1) * 5) + i < maxQuestions)
                {
                    labels[i].Text = (((currentPage - 1) * 5) + (i + 1)) + ". " + questions[((currentPage - 1) * 5) + i];
                    labels[i].Visible = true;
                    float cpt = 0.01f;
                    while (labels[i].Width > 610)
                    {
                        labels[i].Font = new System.Drawing.Font("Tahoma", labels[i].Font.Size - cpt);
                        cpt += 0.01f;
                    }
                    checkedList[i].Visible = true;
                }
                else
                {
                    checkedList[i].Visible = false;
                    labels[i].Visible = false;
                }
                checkedList[i].ClearSelected();
                for (int j = 0; j < 4; j++)
                {
                    checkedList[i].SetItemChecked(j, false);
                }
                if ((((currentPage - 1) * 5) + i) < maxQuestions && answers[(((currentPage - 1) * 5) + i)] != -1)
                    checkedList[i].SetItemChecked(answers[(((currentPage - 1) * 5) + i)], true);
            }

        }

        private void checkEnabled()
        {
            if (currentPage == 1)
            {
                navigPrevious.Enabled = false;
                previous.Enabled = false;
            }
            else
            {
                navigPrevious.Enabled = true;
                previous.Enabled = true;
            }

            if (currentPage == maxPage)
            {
                next.Enabled = false;
                navigNext.Enabled = false;
            }
            else
            {
                next.Enabled = true;
                navigNext.Enabled = true;
            }
        }
        #endregion

        #region Checking
        private void checkAnswer(CheckedListBox clb, int ind)
        {
            if (clb.SelectedIndex != -1)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (i != clb.SelectedIndex && clb.GetItemChecked(i))
                        clb.SetItemChecked(i, false);
                }
                clb.ClearSelected();
            }
        }
        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkAnswer(checkedListBox1, 1);
        }

        private void checkedListBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkAnswer(checkedListBox2, 2);
        }

        private void checkedListBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkAnswer(checkedListBox3, 3);
        }

        private void checkedListBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkAnswer(checkedListBox4, 4);
        }

        private void checkedListBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkAnswer(checkedListBox5, 5);
        }
        #endregion

        #region Last screen
        private void end_Click(object sender, EventArgs e)
        {
            if (allChecked(true))
            {

                saveChart.Visible = true;
                zoomChart.Visible = true;
                showLegend.Visible = true;

                notes.Visible = true;
                notesBox.Visible = true;
                previous.Visible = false;
                next.Visible = false;
                end.Visible = false;
                restart.Visible = true;
                for (int i = 0; i < 5; i++)
                {
                    labels[i].Visible = false;
                    checkedList[i].Visible = false;
                }
                chart1.Visible = true;
                if (autoSave.Checked)
                    save_Click(sender, e);
                try
                {
                    UpdateChart();
                }
                catch (Exception error)  
                {
                    Clipboard.SetText(error.ToString());
                    MessageBox.Show("Une erreur vient de se produire. Pour plus de stabilité, cliquez sur OK puis enregistrer votre travail.\nVeuillez transmettre cette erreur (elle est automatiquement copiée dans le presse-papier, il vous suffit de la coller) :\n" + error,
                "Erreur",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void restart_Click(object sender, EventArgs e)
        {
            saveChart.Visible = false;
            zoomChart.Visible = false;
            showLegend.Visible = false;

            end.Visible = true;
            notes.Visible = false;
            notesBox.Visible = false;
            previous.Visible = true;
            next.Visible = true;
            restart.Visible = false;
            for (int i = 0; i < 5; i++)
            {
                labels[i].Visible = true;
                checkedList[i].Visible = true;
            }
            chart1.Visible = false;

            nameBox.Text = "";
            eraseAllToolStripMenuItem_Click(null, null);
            date.Visible = false;

            currentPage = 1;
            checkEnabled();
            newQuestions();
        }

        private void UpdateChart()
        {
            chart1.Palette = ChartColorPalette.None; 
            chart1.PaletteCustomColors = new Color[]{Color.FromArgb(5,100,146), Color.FromArgb(134,173,0), Color.FromArgb(252,180,65), Color.FromArgb(224,64,10)};
            chart1.Titles.Clear();
            chart1.Titles.Add("Vue globale des réponses au questionnaire : ");
            chart1.Titles[0].Text += String.Format("\"{0}\"", qPath.Substring(0, qPath.Length - 4));
            chart1.Titles[0].Font = new System.Drawing.Font("Tahoma", (float)10);
            chart1.Series.Clear();
            chart1.ChartAreas.Clear();

            chart1.Legends.Clear();
            chart1.Legends.Add(initLegend);
            // Data arrays.
            seriesArray = new string[]{ "A", "B", "C", "D" };// "Pas du tout d'accord", "Pas d'accord", "D'accord", "Tout à fait d'accord" };
            totalAnswers = new int[maxQuestions, 4];
            for (int i = 0; i < maxQuestions; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (answers[i] == j)
                        totalAnswers[i, j] = 1;
                    else
                        totalAnswers[i, j] = 0;
                }
            }
            string currFolder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string[] names = Directory.GetFiles(currFolder + String.Format(@"\FICHIERS_{0}\", qPath.Substring(0, qPath.Length - 4), nameBox.Text), "*.slr");
            for (int i = 0; i < names.Length; i++)
            {
                string temp = names[i].Substring(names[i].LastIndexOf('\\') + 1, (names[i].LastIndexOf('.') - names[i].LastIndexOf('\\') - 1));
                if (temp != nameBox.Text)
                {
                    using (StreamReader reader = new StreamReader(String.Format("FICHIERS_{0}/{1}.slr", qPath.Substring(0, qPath.Length - 4), temp)))
                    {
                        reader.ReadLine();
                        for (int j = 0; j < maxQuestions; j++)
                        {
                            totalAnswers[j, Int32.Parse(reader.ReadLine())]++;
                        }
                    }
                }
            }
            number = names.Length;
            if (nameBox.Text == "")
                number++;

            chart1.Titles[0].Text += String.Format(" (sur {0} personnes)", number);
            for (int i = 0; i < maxQuestions; i++)
            {
                chart1.ChartAreas.Add("Q" + i);
                chart1.Legends.Add("Question" + i);
                chart1.Legends[i + 1].Title = "Question" + i;
                chart1.Legends[i + 1].IsDockedInsideChartArea = true;
                chart1.Legends[i + 1].DockedToChartArea = chart1.ChartAreas[i].Name;

                // Add series.
                string name = String.Format("Q{0}", i + 1);
                chart1.Series.Add(name);
                for (int j = 0; j < 4; j++)
                {// Add point.
                    chart1.Series[name].ChartArea = chart1.ChartAreas[i].Name;
                    chart1.Series[name].ChartType = SeriesChartType.Pie;
                    chart1.Series[name].Points.Add(Math.Round((100 * ((float)totalAnswers[i, j] / number))));

                    chart1.Series[name].Points[j].LegendText = String.Format("{0}{1} : {2}%", name, seriesArray[j], chart1.Series[name].Points[j].YValues[0]);
                }
            
            }
            chart1.Legends[0].Enabled = false;
        }
        #endregion

        #region Quitting

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            quit_Click(sender, e);
        }

        private void quit_Click(object sender, EventArgs e)
        {
            int i;
            write_config();
            for (i = 0; i < 5; i++)
            {
                int j;
                for (j = 0; j < 4; j++)
                {
                    if (checkedList[i].GetItemChecked(j))
                        break;
                }
                if (j != 4)
                    break;
            }
            if (i != 5)
            {
                if (nameBox.Text != "")
                {
                    DialogResult result = MessageBox.Show("Vous avez apporté des modifications à ce document :\nSouhaitez-vous sauvegarder avant de quitter ?",
                "Avertissement",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1);

                    switch (result)
                    {
                        case System.Windows.Forms.DialogResult.Yes:
                            save_Click(sender, e);
                            Environment.Exit(1);
                            break;
                        case System.Windows.Forms.DialogResult.No:
                            Environment.Exit(1);
                            break;
                        case System.Windows.Forms.DialogResult.Cancel:
                            break;
                    }
                }
                else
                {
                    DialogResult result = MessageBox.Show("Vous avez apporté des modifications à ce document :\nVous êtes sur le point de quitter sans sauvegarder.\nCliquez sur Annuler pour retourner au logiciel. OK sinon.",
                "Avertissement",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1);

                    switch (result)
                    {
                        case System.Windows.Forms.DialogResult.OK:
                            Environment.Exit(1);
                            break;
                        case System.Windows.Forms.DialogResult.Cancel:
                            break;
                    }
                }
            }
            else
                Environment.Exit(1);
        }

        #endregion

        #region Saving/Loading
        private void save_Click(object sender, EventArgs e)
        {
            if (nameBox.Text != "")
            {
                using (StreamWriter writer = new StreamWriter(String.Format("FICHIERS_{0}/{1}.slr", qPath.Substring(0, qPath.Length - 4), nameBox.Text)))
                {
                    writer.WriteLine(DateTime.Today.ToShortDateString());
                    for (int i = 0; i < maxQuestions; i++)
                    {
                        writer.WriteLine(answers[i]);
                    }
                    notesBox.Text = notesBox.Text.Trim();
                    writer.Write(notesBox.Text);
                }
            }
        }

        private void load_Click(object sender, EventArgs e)
        {
            restart_Click(null, null);
            load_client(nameBox.Text);
        }
        private void nameBox_Click(object sender, EventArgs e)
        {
            restart_Click(null, null);
            load_client((string)nameBox.SelectedItem);
        }
        private void load_client(string txt)
        {
            if (txt != "")
            {
                try
                {
                    using (StreamReader reader = new StreamReader(String.Format("FICHIERS_{0}/{1}.slr", qPath.Substring(0, qPath.Length - 4), txt)))
                    {
                        date.Visible = true;
                        date.Text = "Précédente Édition : " + reader.ReadLine();
                        for (int i = 0; i < maxQuestions; i++)
                        {
                            answers[i] = Int32.Parse(reader.ReadLine());
                        }
                        notesBox.Text = reader.ReadToEnd();
                    }
                    newQuestions();
                }
                catch
                {
                    MessageBox.Show("Aucun fichier existant ne correspond à ce salarié ou celui précédemment enregistré a été modifié ou corrompu.",
                        "Aucun fichier existant ou le fichier est corrompu.",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation,
                        MessageBoxDefaultButton.Button1);
                }
            }
        }
        #endregion

        private void nameBox_Dropdown(object sender, EventArgs e)
        {
            nameBox.Items.Clear();
            nameBox.Height = 0;
            string currFolder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string[] names = Directory.GetFiles(currFolder + String.Format(@"\FICHIERS_{0}\", qPath.Substring(0, qPath.Length - 4), nameBox.Text), "*.slr");
            foreach (string name in names)
            {
                string temp = name.Substring(name.LastIndexOf('\\') + 1, (name.LastIndexOf('.') - name.LastIndexOf('\\') - 1));
                nameBox.Items.Add(temp);
                nameBox.Height += 20;
            }
        }

        #region Toolbar
        private void about_Click(object sender, EventArgs e)
        {
            DialogResult result2 = MessageBox.Show("MCQHelper est un logiciel d'aide au recensement et à la sauvegarde des données.\nCréé par Roman SAHEL et protégé par les droits d'auteur.\n\nPour créer un nouveau questionnaire, il suffit de créer un fichier texte avec une question par ligne. Puis, ouvrez-le avec 'Changer de Questionnaire'",
                "À propos",
                MessageBoxButtons.OK,
                MessageBoxIcon.Question);
        }

        private void eraseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < maxQuestions; i++)
            {
                answers[i] = -1;
            }
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    checkedList[i].SetItemChecked(j, false);
                }
            }
        }

        private void changeMCQ_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                qPath = openFileDialog1.FileName.Substring(openFileDialog1.FileName.LastIndexOf('\\') + 1, (openFileDialog1.FileName.LastIndexOf('.') - openFileDialog1.FileName.LastIndexOf('\\') - 1));
                qPath += ".txt";
                string file;
                using (StreamReader reader = new StreamReader(openFileDialog1.FileName))
                {
                    file = reader.ReadToEnd();
                }
                using (StreamWriter writer = new StreamWriter(qPath))
                {
                    writer.WriteLine(file);
                }
                write_config();
                read_config();
                read_Questions();
                if (creationBox.Visible)
                {
                    newMCQbox.Visible = !newMCQbox.Visible;
                    creationBox.Visible = !creationBox.Visible;
                    saveNewMCQ.Visible = !saveNewMCQ.Visible;
                }
                restart_Click(null, null);
            }
            else if (qPath == "")
                quit_Click(sender, e);
        }

        private void insérerLaDateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (notesBox.Visible)
                notesBox.Text += String.Format("[{0}]", DateTime.Today.ToShortDateString());
        }



        private void zoom_Click(object sender, EventArgs e)
        {
            if (chart1.Dock == DockStyle.Fill)
            {
                chart1.Dock = DockStyle.None;
                saveChart.Location = new System.Drawing.Point(saveChart.Location.X - 16, saveChart.Location.Y);
                zoomChart.Location = new System.Drawing.Point(zoomChart.Location.X - 16, zoomChart.Location.Y);
                showLegend.Location = new System.Drawing.Point(showLegend.Location.X - 16, showLegend.Location.Y);
            }
            else
            {
                chart1.Dock = DockStyle.Fill;
                saveChart.Location = new System.Drawing.Point(saveChart.Location.X + 16, saveChart.Location.Y);
                zoomChart.Location = new System.Drawing.Point(zoomChart.Location.X + 16, zoomChart.Location.Y);
                showLegend.Location = new System.Drawing.Point(showLegend.Location.X + 16, showLegend.Location.Y);
            }
        }

        private void saveChart_Click(object sender, EventArgs e)
        {
            //DockStyle d = chart1.Dock;

            //chart1.Dock = DockStyle.None;

            //int w = chart1.Width, h = chart1.Height;
            //System.Drawing.Font f = chart1.Legends[0].Font;
            //bool b = chart1.Legends[0].Enabled;

            //chart1.Legends[0].Enabled = true;

            //chart1.Titles[0].Font = new System.Drawing.Font("Tahoma", chart1.Titles[0].Font.Size * 10);
            //chart1.Legends[0].Font = new System.Drawing.Font("Arial", 60);
            
            //chart1.Width =  100;
            //chart1.Height = 12000;

            string currFolder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "/";
            string filename = currFolder + qPath.Substring(0, qPath.Length - 4) + "_VUEGLOBALE.html";
            string img_folder = String.Format("FICHIERS_{0}/img/", qPath.Substring(0, qPath.Length - 4), nameBox.Text);

            bool IsExists = System.IO.Directory.Exists(img_folder);
            if (!IsExists)
                System.IO.Directory.CreateDirectory(img_folder);

            //chart1.Titles[0].Font = new System.Drawing.Font("Tahoma", chart1.Titles[0].Font.Size / 10);
            //chart1.Legends[0].Enabled = b;
            //chart1.Legends[0].Font = f;
            //chart1.Width = w;
            //chart1.Height = h;
            //chart1.Dock = d;


            using (StreamWriter writer = new StreamWriter(@filename, false, Encoding.UTF8))
            {
                writer.WriteLine("<head><style type= \"text/css\">");
                writer.WriteLine("* {font-family:\"Arial\", Times, serif;}");
                writer.WriteLine("h1 {text-align:center; font-size: 3em;}");
                writer.WriteLine("h2 {font-size: 2em;}");
                writer.WriteLine("ul {margin-top: 0; list-style-type: none;  font-size: 1.5em; display: inline-block;}");
                writer.WriteLine("li {width:300px; margin-left:50px; text-align:right; line-height: 1.5;}");
                writer.WriteLine("img { width: 150px; margin-left: 100px; display: inline-block; }");

                writer.WriteLine("span{ display: block; float: left; width:10px; height: 10px; margin: 10px 10px 0 0 ; background: #FFFFFF;}");
                writer.WriteLine("ul :nth-child(1) span{ background: #056492; }");
                writer.WriteLine("ul :nth-child(2) span{ background: #86AD00; }");
                writer.WriteLine("ul :nth-child(3) span{ background: #fcb441; }");
                writer.WriteLine("ul :nth-child(4) span{ background: #e0400a; }");
                writer.WriteLine("</style></head>");

                writer.WriteLine("<body>");
                writer.WriteLine(String.Format("<h1>{0}<br>({1}</h1>", chart1.Titles[0].Text.Split('(')[0], chart1.Titles[0].Text.Split('(')[1]));
                for (int i = 0; i < maxQuestions; i++)
                {
                    writer.WriteLine(String.Format("<h2>{0}. {1}</h2>", (i + 1), questions[i]));
                    writer.WriteLine("<div>\n<ul>");

                    Chart temp = new Chart();
                    temp.Palette = ChartColorPalette.None;
                    temp.PaletteCustomColors = new Color[] { Color.FromArgb(5, 100, 146), Color.FromArgb(134, 173, 0), Color.FromArgb(252, 180, 65), Color.FromArgb(224, 64, 10) };
                    temp.Width = 450;
                    temp.Height = 450;

                    temp.ChartAreas.Add("Q" + i);
                    string name = String.Format("Q{0}", i + 1);
                    temp.Series.Add(name);

                    for (int j = 0; j < 4; j++)
                    {
                        writer.WriteLine(String.Format("<li><span></span>{0} : {1}</li>", checkedList[0].Items[j], (chart1.Series[i].Points[j].LegendText.Split(':'))[1]));

                        temp.Series[name].ChartArea = chart1.ChartAreas[i].Name;
                        temp.Series[name].ChartType = SeriesChartType.Pie;
                        temp.Series[name].Points.Add(Math.Round((100 * ((float)totalAnswers[i, j] / number))));

                        temp.Series[name].Points[j].LegendText = String.Format("{0}{1} : {2}%", name, seriesArray[j], temp.Series[name].Points[j].YValues[0]);
                    }
                    temp.SaveImage(@currFolder + @img_folder + i + ".jpg", ChartImageFormat.Jpeg);
                    writer.WriteLine("</ul><img src=\"" + @img_folder + i + ".jpg\"</img>\n</div>");
                }
                writer.WriteLine("</body>");
            }


            System.Diagnostics.Process.Start(@filename);
        }

        private void showLegend_Click(object sender, EventArgs e)
        {
            chart1.Legends[0].Enabled = !chart1.Legends[0].Enabled;
        }

        private void CreateMCQ_Click(object sender, EventArgs e)
        {
            if (creationBox.Visible)
            {
                if (newMCQbox.Text == "" || newMCQbox.Text == "Nom du QCM")
                {
                    MessageBox.Show("Veuillez entrer un nom de questionnaire dans le champ prévu à cet effet (à droite du menu 'Options')",
                      "Veuillez entrer un nom de questionnaire !",
                      MessageBoxButtons.OK,
                      MessageBoxIcon.Error,
                      MessageBoxDefaultButton.Button2);
                }
                else
                {
                    qPath = newMCQbox.Text + ".txt";
                    using (StreamWriter writer = new StreamWriter(qPath))
                    {
                        writer.Write(creationBox.Text);
                    }
                    write_config();
                    read_config();
                    read_Questions();
                    newMCQbox.Visible = !newMCQbox.Visible;
                    creationBox.Visible = !creationBox.Visible;
                    saveNewMCQ.Visible = !saveNewMCQ.Visible;
                    checkEnabled();
                }
            }
            else
            {
                restart_Click(null, null);

                navigNext.Enabled = false;
                navigEnd.Enabled = false;
                navigPrevious.Enabled = false;

                newMCQbox.Visible = !newMCQbox.Visible;
                creationBox.Visible = !creationBox.Visible;
                saveNewMCQ.Visible = !saveNewMCQ.Visible;
            }

        }

        private void newMCQbox_Click(object sender, EventArgs e)
        {
            newMCQbox.Text = "";
        }
        #endregion

        #region Init
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea4 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea5 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea6 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea7 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea8 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea9 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea10 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea11 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea12 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea13 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea14 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea15 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea16 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea17 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea18 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea19 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea20 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea21 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea22 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea23 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea24 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea25 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea26 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea27 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea28 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea29 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea30 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea31 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea32 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea33 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea34 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.DataPoint dataPoint1 = new System.Windows.Forms.DataVisualization.Charting.DataPoint(0D, 33D);
            System.Windows.Forms.DataVisualization.Charting.DataPoint dataPoint2 = new System.Windows.Forms.DataVisualization.Charting.DataPoint(0D, 33D);
            System.Windows.Forms.DataVisualization.Charting.DataPoint dataPoint3 = new System.Windows.Forms.DataVisualization.Charting.DataPoint(0D, 22D);
            System.Windows.Forms.DataVisualization.Charting.DataPoint dataPoint4 = new System.Windows.Forms.DataVisualization.Charting.DataPoint(0D, 22D);
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series5 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series6 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series7 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series8 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series9 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series10 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series11 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series12 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series13 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series14 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series15 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series16 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series17 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series18 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series19 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series20 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series21 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series22 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series23 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series24 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series25 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series26 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series27 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series28 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series29 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series30 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series31 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series32 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series33 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.labelname = new System.Windows.Forms.Label();
            this.save = new System.Windows.Forms.Button();
            this.load = new System.Windows.Forms.Button();
            this.date = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.checkedListBox2 = new System.Windows.Forms.CheckedListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.checkedListBox3 = new System.Windows.Forms.CheckedListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.checkedListBox5 = new System.Windows.Forms.CheckedListBox();
            this.label5 = new System.Windows.Forms.Label();
            this.checkedListBox4 = new System.Windows.Forms.CheckedListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.previous = new System.Windows.Forms.Button();
            this.next = new System.Windows.Forms.Button();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.restart = new System.Windows.Forms.Button();
            this.Fichier = new System.Windows.Forms.ToolStripMenuItem();
            this.ouvrirToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enregistrerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Édition = new System.Windows.Forms.ToolStripMenuItem();
            this.eraseAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.changerDeQuestionnaireToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.about = new System.Windows.Forms.ToolStripMenuItem();
            this.autoSave = new System.Windows.Forms.ToolStripMenuItem();
            this.nameBox = new System.Windows.Forms.ComboBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
            this.insérerLaDateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.navigationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.navigNext = new System.Windows.Forms.ToolStripMenuItem();
            this.navigPrevious = new System.Windows.Forms.ToolStripMenuItem();
            this.navigEnd = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem10 = new System.Windows.Forms.ToolStripMenuItem();
            this.newMCQ = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem11 = new System.Windows.Forms.ToolStripMenuItem();
            this.saveNewMCQ = new System.Windows.Forms.ToolStripMenuItem();
            this.newMCQbox = new System.Windows.Forms.ToolStripTextBox();
            this.notesBox = new System.Windows.Forms.RichTextBox();
            this.notes = new System.Windows.Forms.Label();
            this.saveChart = new System.Windows.Forms.Button();
            this.showLegend = new System.Windows.Forms.Button();
            this.zoomChart = new System.Windows.Forms.Button();
            this.end = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.creationBox = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelname
            // 
            this.labelname.AutoSize = true;
            this.labelname.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelname.Location = new System.Drawing.Point(12, 28);
            this.labelname.Name = "labelname";
            this.labelname.Size = new System.Drawing.Size(103, 16);
            this.labelname.TabIndex = 0;
            this.labelname.Text = "Nom du salarié :";
            // 
            // save
            // 
            this.save.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.save.Location = new System.Drawing.Point(332, 23);
            this.save.Name = "save";
            this.save.Size = new System.Drawing.Size(98, 26);
            this.save.TabIndex = 2;
            this.save.Text = "Enregistrer";
            this.save.UseVisualStyleBackColor = true;
            this.save.Click += new System.EventHandler(this.save_Click);
            // 
            // load
            // 
            this.load.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.load.Location = new System.Drawing.Point(440, 23);
            this.load.Name = "load";
            this.load.Size = new System.Drawing.Size(98, 26);
            this.load.TabIndex = 3;
            this.load.Text = "Charger";
            this.load.UseVisualStyleBackColor = true;
            this.load.Click += new System.EventHandler(this.load_Click);
            // 
            // date
            // 
            this.date.AutoSize = true;
            this.date.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.date.Location = new System.Drawing.Point(329, 52);
            this.date.Name = "date";
            this.date.Size = new System.Drawing.Size(193, 16);
            this.date.TabIndex = 4;
            this.date.Text = "Précédente Édition : 00/00/0000";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 69);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 18);
            this.label1.TabIndex = 5;
            this.label1.Text = "label9";
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.checkedListBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.checkedListBox1.CheckOnClick = true;
            this.checkedListBox1.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkedListBox1.FormattingEnabled = true;
            this.checkedListBox1.Items.AddRange(new object[] {
            "A. Réponse A",
            "B. Réponse B",
            "C. Réponse C",
            "D. Réponse D"});
            this.checkedListBox1.Location = new System.Drawing.Point(53, 92);
            this.checkedListBox1.Name = "checkedListBox1";
            this.checkedListBox1.Size = new System.Drawing.Size(426, 84);
            this.checkedListBox1.TabIndex = 6;
            this.checkedListBox1.SelectedIndexChanged += new System.EventHandler(this.checkedListBox1_SelectedIndexChanged);
            // 
            // checkedListBox2
            // 
            this.checkedListBox2.BackColor = System.Drawing.Color.WhiteSmoke;
            this.checkedListBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.checkedListBox2.CheckOnClick = true;
            this.checkedListBox2.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkedListBox2.FormattingEnabled = true;
            this.checkedListBox2.Items.AddRange(new object[] {
            "A. Réponse A",
            "B. Réponse B",
            "C. Réponse C",
            "D. Réponse D"});
            this.checkedListBox2.Location = new System.Drawing.Point(53, 202);
            this.checkedListBox2.Name = "checkedListBox2";
            this.checkedListBox2.Size = new System.Drawing.Size(426, 84);
            this.checkedListBox2.TabIndex = 8;
            this.checkedListBox2.SelectedIndexChanged += new System.EventHandler(this.checkedListBox2_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 179);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 18);
            this.label2.TabIndex = 7;
            this.label2.Text = "label10";
            // 
            // checkedListBox3
            // 
            this.checkedListBox3.BackColor = System.Drawing.Color.WhiteSmoke;
            this.checkedListBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.checkedListBox3.CheckOnClick = true;
            this.checkedListBox3.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkedListBox3.FormattingEnabled = true;
            this.checkedListBox3.Items.AddRange(new object[] {
            "A. Réponse A",
            "B. Réponse B",
            "C. Réponse C",
            "D. Réponse D"});
            this.checkedListBox3.Location = new System.Drawing.Point(53, 312);
            this.checkedListBox3.Name = "checkedListBox3";
            this.checkedListBox3.Size = new System.Drawing.Size(426, 84);
            this.checkedListBox3.TabIndex = 10;
            this.checkedListBox3.SelectedIndexChanged += new System.EventHandler(this.checkedListBox3_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(12, 289);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 18);
            this.label3.TabIndex = 9;
            this.label3.Text = "label11";
            // 
            // checkedListBox5
            // 
            this.checkedListBox5.BackColor = System.Drawing.Color.WhiteSmoke;
            this.checkedListBox5.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.checkedListBox5.CheckOnClick = true;
            this.checkedListBox5.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkedListBox5.FormattingEnabled = true;
            this.checkedListBox5.Items.AddRange(new object[] {
            "A. Réponse A",
            "B. Réponse B",
            "C. Réponse C",
            "D. Réponse D"});
            this.checkedListBox5.Location = new System.Drawing.Point(53, 531);
            this.checkedListBox5.Name = "checkedListBox5";
            this.checkedListBox5.Size = new System.Drawing.Size(426, 84);
            this.checkedListBox5.TabIndex = 16;
            this.checkedListBox5.SelectedIndexChanged += new System.EventHandler(this.checkedListBox5_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(12, 508);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(52, 18);
            this.label5.TabIndex = 15;
            this.label5.Text = "label12";
            // 
            // checkedListBox4
            // 
            this.checkedListBox4.BackColor = System.Drawing.Color.WhiteSmoke;
            this.checkedListBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.checkedListBox4.CheckOnClick = true;
            this.checkedListBox4.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkedListBox4.FormattingEnabled = true;
            this.checkedListBox4.Items.AddRange(new object[] {
            "A. Réponse A",
            "B. Réponse B",
            "C. Réponse C",
            "D. Réponse D"});
            this.checkedListBox4.Location = new System.Drawing.Point(53, 422);
            this.checkedListBox4.Name = "checkedListBox4";
            this.checkedListBox4.Size = new System.Drawing.Size(426, 84);
            this.checkedListBox4.TabIndex = 14;
            this.checkedListBox4.SelectedIndexChanged += new System.EventHandler(this.checkedListBox4_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(12, 399);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 18);
            this.label4.TabIndex = 13;
            this.label4.Text = "label13";
            // 
            // previous
            // 
            this.previous.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.previous.Location = new System.Drawing.Point(12, 649);
            this.previous.Name = "previous";
            this.previous.Size = new System.Drawing.Size(136, 23);
            this.previous.TabIndex = 17;
            this.previous.Text = "Précédent";
            this.previous.UseVisualStyleBackColor = true;
            this.previous.Click += new System.EventHandler(this.previous_Click);
            // 
            // next
            // 
            this.next.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.next.Location = new System.Drawing.Point(459, 649);
            this.next.Name = "next";
            this.next.Size = new System.Drawing.Size(136, 23);
            this.next.TabIndex = 18;
            this.next.Text = "Suivant";
            this.next.UseVisualStyleBackColor = true;
            this.next.Click += new System.EventHandler(this.next_Click);
            // 
            // chart1
            // 
            this.chart1.BorderlineColor = System.Drawing.Color.Gainsboro;
            this.chart1.BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
            this.chart1.BorderlineWidth = 2;
            chartArea1.AlignmentOrientation = System.Windows.Forms.DataVisualization.Charting.AreaAlignmentOrientations.None;
            chartArea1.AlignmentStyle = System.Windows.Forms.DataVisualization.Charting.AreaAlignmentStyles.Cursor;
            chartArea1.AlignWithChartArea = "ChartArea1";
            chartArea1.CursorX.AxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
            chartArea1.Name = "ChartArea1";
            chartArea1.Position.Auto = false;
            chartArea1.Position.Height = 10.85714F;
            chartArea1.Position.Width = 6.415942F;
            chartArea1.Position.X = 3F;
            chartArea1.Position.Y = 3F;
            chartArea2.AlignmentOrientation = System.Windows.Forms.DataVisualization.Charting.AreaAlignmentOrientations.None;
            chartArea2.AlignmentStyle = System.Windows.Forms.DataVisualization.Charting.AreaAlignmentStyles.Cursor;
            chartArea2.AlignWithChartArea = "ChartArea1";
            chartArea2.InnerPlotPosition.Auto = false;
            chartArea2.InnerPlotPosition.Height = 66.64301F;
            chartArea2.InnerPlotPosition.Width = 95F;
            chartArea2.InnerPlotPosition.X = 2.499999F;
            chartArea2.InnerPlotPosition.Y = 16.6785F;
            chartArea2.Name = "ChartArea2";
            chartArea2.Position.Auto = false;
            chartArea2.Position.Height = 10.85714F;
            chartArea2.Position.Width = 6.415942F;
            chartArea2.Position.X = 3F;
            chartArea2.Position.Y = 16.85714F;
            chartArea3.AlignmentOrientation = System.Windows.Forms.DataVisualization.Charting.AreaAlignmentOrientations.None;
            chartArea3.AlignmentStyle = System.Windows.Forms.DataVisualization.Charting.AreaAlignmentStyles.Cursor;
            chartArea3.AlignWithChartArea = "ChartArea1";
            chartArea3.InnerPlotPosition.Auto = false;
            chartArea3.InnerPlotPosition.Height = 66.64301F;
            chartArea3.InnerPlotPosition.Width = 95F;
            chartArea3.InnerPlotPosition.X = 2.499999F;
            chartArea3.InnerPlotPosition.Y = 16.6785F;
            chartArea3.Name = "ChartArea3";
            chartArea3.Position.Auto = false;
            chartArea3.Position.Height = 10.85714F;
            chartArea3.Position.Width = 6.415942F;
            chartArea3.Position.X = 3F;
            chartArea3.Position.Y = 30.71428F;
            chartArea4.AlignmentOrientation = System.Windows.Forms.DataVisualization.Charting.AreaAlignmentOrientations.None;
            chartArea4.AlignmentStyle = System.Windows.Forms.DataVisualization.Charting.AreaAlignmentStyles.Cursor;
            chartArea4.AlignWithChartArea = "ChartArea1";
            chartArea4.InnerPlotPosition.Auto = false;
            chartArea4.InnerPlotPosition.Height = 66.64301F;
            chartArea4.InnerPlotPosition.Width = 95F;
            chartArea4.InnerPlotPosition.X = 2.499999F;
            chartArea4.InnerPlotPosition.Y = 16.67848F;
            chartArea4.Name = "ChartArea4";
            chartArea4.Position.Auto = false;
            chartArea4.Position.Height = 10.85714F;
            chartArea4.Position.Width = 6.415942F;
            chartArea4.Position.X = 3F;
            chartArea4.Position.Y = 44.57143F;
            chartArea5.Name = "ChartArea5";
            chartArea5.Position.Auto = false;
            chartArea5.Position.Height = 10.85714F;
            chartArea5.Position.Width = 6.415942F;
            chartArea5.Position.X = 3F;
            chartArea5.Position.Y = 58.42857F;
            chartArea6.Name = "ChartArea6";
            chartArea6.Position.Auto = false;
            chartArea6.Position.Height = 10.85714F;
            chartArea6.Position.Width = 6.415942F;
            chartArea6.Position.X = 3F;
            chartArea6.Position.Y = 72.28571F;
            chartArea7.Name = "ChartArea7";
            chartArea7.Position.Auto = false;
            chartArea7.Position.Height = 10.85714F;
            chartArea7.Position.Width = 6.415942F;
            chartArea7.Position.X = 3F;
            chartArea7.Position.Y = 86.14286F;
            chartArea8.Name = "ChartArea8";
            chartArea8.Position.Auto = false;
            chartArea8.Position.Height = 10.85714F;
            chartArea8.Position.Width = 6.415942F;
            chartArea8.Position.X = 12.41594F;
            chartArea8.Position.Y = 3F;
            chartArea9.Name = "ChartArea9";
            chartArea9.Position.Auto = false;
            chartArea9.Position.Height = 10.85714F;
            chartArea9.Position.Width = 6.415942F;
            chartArea9.Position.X = 12.41594F;
            chartArea9.Position.Y = 16.85714F;
            chartArea10.Name = "ChartArea10";
            chartArea10.Position.Auto = false;
            chartArea10.Position.Height = 10.85714F;
            chartArea10.Position.Width = 6.415942F;
            chartArea10.Position.X = 12.41594F;
            chartArea10.Position.Y = 30.71428F;
            chartArea11.Name = "ChartArea11";
            chartArea11.Position.Auto = false;
            chartArea11.Position.Height = 10.85714F;
            chartArea11.Position.Width = 6.415942F;
            chartArea11.Position.X = 12.41594F;
            chartArea11.Position.Y = 44.57143F;
            chartArea12.Name = "ChartArea12";
            chartArea12.Position.Auto = false;
            chartArea12.Position.Height = 10.85714F;
            chartArea12.Position.Width = 6.415942F;
            chartArea12.Position.X = 12.41594F;
            chartArea12.Position.Y = 58.42857F;
            chartArea13.Name = "ChartArea13";
            chartArea13.Position.Auto = false;
            chartArea13.Position.Height = 10.85714F;
            chartArea13.Position.Width = 6.415942F;
            chartArea13.Position.X = 12.41594F;
            chartArea13.Position.Y = 72.28571F;
            chartArea14.Name = "ChartArea14";
            chartArea14.Position.Auto = false;
            chartArea14.Position.Height = 10.85714F;
            chartArea14.Position.Width = 6.415942F;
            chartArea14.Position.X = 12.41594F;
            chartArea14.Position.Y = 86.14286F;
            chartArea15.Name = "ChartArea15";
            chartArea15.Position.Auto = false;
            chartArea15.Position.Height = 10.85714F;
            chartArea15.Position.Width = 6.415942F;
            chartArea15.Position.X = 21.83188F;
            chartArea15.Position.Y = 3F;
            chartArea16.Name = "ChartArea16";
            chartArea16.Position.Auto = false;
            chartArea16.Position.Height = 10.85714F;
            chartArea16.Position.Width = 6.415942F;
            chartArea16.Position.X = 21.83188F;
            chartArea16.Position.Y = 16.85714F;
            chartArea17.Name = "ChartArea17";
            chartArea17.Position.Auto = false;
            chartArea17.Position.Height = 10.85714F;
            chartArea17.Position.Width = 6.415942F;
            chartArea17.Position.X = 21.83188F;
            chartArea17.Position.Y = 30.71428F;
            chartArea18.Name = "ChartArea18";
            chartArea18.Position.Auto = false;
            chartArea18.Position.Height = 10.85714F;
            chartArea18.Position.Width = 6.415942F;
            chartArea18.Position.X = 21.83188F;
            chartArea18.Position.Y = 44.57143F;
            chartArea19.Name = "ChartArea19";
            chartArea19.Position.Auto = false;
            chartArea19.Position.Height = 10.85714F;
            chartArea19.Position.Width = 6.415942F;
            chartArea19.Position.X = 21.83188F;
            chartArea19.Position.Y = 58.42857F;
            chartArea20.Name = "ChartArea20";
            chartArea20.Position.Auto = false;
            chartArea20.Position.Height = 10.85714F;
            chartArea20.Position.Width = 6.415942F;
            chartArea20.Position.X = 21.83188F;
            chartArea20.Position.Y = 72.28571F;
            chartArea21.Name = "ChartArea21";
            chartArea21.Position.Auto = false;
            chartArea21.Position.Height = 10.85714F;
            chartArea21.Position.Width = 6.415942F;
            chartArea21.Position.X = 21.83188F;
            chartArea21.Position.Y = 86.14286F;
            chartArea22.Name = "ChartArea22";
            chartArea22.Position.Auto = false;
            chartArea22.Position.Height = 10.85714F;
            chartArea22.Position.Width = 6.415942F;
            chartArea22.Position.X = 31.24783F;
            chartArea22.Position.Y = 3F;
            chartArea23.Name = "ChartArea23";
            chartArea23.Position.Auto = false;
            chartArea23.Position.Height = 10.85714F;
            chartArea23.Position.Width = 6.415942F;
            chartArea23.Position.X = 31.24783F;
            chartArea23.Position.Y = 16.85714F;
            chartArea24.Name = "ChartArea24";
            chartArea24.Position.Auto = false;
            chartArea24.Position.Height = 10.85714F;
            chartArea24.Position.Width = 6.415942F;
            chartArea24.Position.X = 31.24783F;
            chartArea24.Position.Y = 30.71428F;
            chartArea25.Name = "ChartArea25";
            chartArea25.Position.Auto = false;
            chartArea25.Position.Height = 10.85714F;
            chartArea25.Position.Width = 6.415942F;
            chartArea25.Position.X = 31.24783F;
            chartArea25.Position.Y = 44.57143F;
            chartArea26.Name = "ChartArea26";
            chartArea26.Position.Auto = false;
            chartArea26.Position.Height = 10.85714F;
            chartArea26.Position.Width = 6.415942F;
            chartArea26.Position.X = 31.24783F;
            chartArea26.Position.Y = 58.42857F;
            chartArea27.Name = "ChartArea27";
            chartArea27.Position.Auto = false;
            chartArea27.Position.Height = 10.85714F;
            chartArea27.Position.Width = 6.415942F;
            chartArea27.Position.X = 31.24783F;
            chartArea27.Position.Y = 72.28571F;
            chartArea28.Name = "ChartArea28";
            chartArea28.Position.Auto = false;
            chartArea28.Position.Height = 10.85714F;
            chartArea28.Position.Width = 6.415942F;
            chartArea28.Position.X = 31.24783F;
            chartArea28.Position.Y = 86.14286F;
            chartArea29.Name = "ChartArea29";
            chartArea29.Position.Auto = false;
            chartArea29.Position.Height = 10.85714F;
            chartArea29.Position.Width = 6.415942F;
            chartArea29.Position.X = 40.66377F;
            chartArea29.Position.Y = 3F;
            chartArea30.Name = "ChartArea30";
            chartArea30.Position.Auto = false;
            chartArea30.Position.Height = 10.85714F;
            chartArea30.Position.Width = 6.415942F;
            chartArea30.Position.X = 40.66377F;
            chartArea30.Position.Y = 16.85714F;
            chartArea31.Name = "ChartArea31";
            chartArea31.Position.Auto = false;
            chartArea31.Position.Height = 10.85714F;
            chartArea31.Position.Width = 6.415942F;
            chartArea31.Position.X = 40.66377F;
            chartArea31.Position.Y = 30.71428F;
            chartArea32.Name = "ChartArea32";
            chartArea32.Position.Auto = false;
            chartArea32.Position.Height = 10.85714F;
            chartArea32.Position.Width = 6.415942F;
            chartArea32.Position.X = 40.66377F;
            chartArea32.Position.Y = 44.57143F;
            chartArea33.Name = "ChartArea33";
            chartArea33.Position.Auto = false;
            chartArea33.Position.Height = 10.85714F;
            chartArea33.Position.Width = 6.415942F;
            chartArea33.Position.X = 40.66377F;
            chartArea33.Position.Y = 58.42857F;
            chartArea34.Name = "ChartArea34";
            chartArea34.Position.Auto = false;
            chartArea34.Position.Height = 10.85714F;
            chartArea34.Position.Width = 6.415942F;
            chartArea34.Position.X = 40.66377F;
            chartArea34.Position.Y = 72.28571F;
            this.chart1.ChartAreas.Add(chartArea1);
            this.chart1.ChartAreas.Add(chartArea2);
            this.chart1.ChartAreas.Add(chartArea3);
            this.chart1.ChartAreas.Add(chartArea4);
            this.chart1.ChartAreas.Add(chartArea5);
            this.chart1.ChartAreas.Add(chartArea6);
            this.chart1.ChartAreas.Add(chartArea7);
            this.chart1.ChartAreas.Add(chartArea8);
            this.chart1.ChartAreas.Add(chartArea9);
            this.chart1.ChartAreas.Add(chartArea10);
            this.chart1.ChartAreas.Add(chartArea11);
            this.chart1.ChartAreas.Add(chartArea12);
            this.chart1.ChartAreas.Add(chartArea13);
            this.chart1.ChartAreas.Add(chartArea14);
            this.chart1.ChartAreas.Add(chartArea15);
            this.chart1.ChartAreas.Add(chartArea16);
            this.chart1.ChartAreas.Add(chartArea17);
            this.chart1.ChartAreas.Add(chartArea18);
            this.chart1.ChartAreas.Add(chartArea19);
            this.chart1.ChartAreas.Add(chartArea20);
            this.chart1.ChartAreas.Add(chartArea21);
            this.chart1.ChartAreas.Add(chartArea22);
            this.chart1.ChartAreas.Add(chartArea23);
            this.chart1.ChartAreas.Add(chartArea24);
            this.chart1.ChartAreas.Add(chartArea25);
            this.chart1.ChartAreas.Add(chartArea26);
            this.chart1.ChartAreas.Add(chartArea27);
            this.chart1.ChartAreas.Add(chartArea28);
            this.chart1.ChartAreas.Add(chartArea29);
            this.chart1.ChartAreas.Add(chartArea30);
            this.chart1.ChartAreas.Add(chartArea31);
            this.chart1.ChartAreas.Add(chartArea32);
            this.chart1.ChartAreas.Add(chartArea33);
            this.chart1.ChartAreas.Add(chartArea34);
            legend1.BackColor = System.Drawing.Color.Transparent;
            legend1.BorderWidth = 0;
            legend1.Font = new System.Drawing.Font("Tahoma", 8F);
            legend1.IsTextAutoFit = false;
            legend1.ItemColumnSpacing = 0;
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(12, 74);
            this.chart1.Name = "chart1";
            this.chart1.PaletteCustomColors = new System.Drawing.Color[] {
        System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(100)))), ((int)(((byte)(146))))),
        System.Drawing.Color.FromArgb(((int)(((byte)(134)))), ((int)(((byte)(173)))), ((int)(((byte)(0))))),
        System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(180)))), ((int)(((byte)(65))))),
        System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(64)))), ((int)(((byte)(10)))))};
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series1.Legend = "Legend1";
            series1.MarkerSize = 1;
            series1.Name = "Series1";
            dataPoint1.IsVisibleInLegend = true;
            dataPoint1.Label = "";
            dataPoint1.LegendText = "Q0-A : 99%";
            series1.Points.Add(dataPoint1);
            series1.Points.Add(dataPoint2);
            series1.Points.Add(dataPoint3);
            series1.Points.Add(dataPoint4);
            series1.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series2.ChartArea = "ChartArea2";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series2.Legend = "Legend1";
            series2.Name = "Series2";
            series2.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series3.ChartArea = "ChartArea3";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series3.Legend = "Legend1";
            series3.Name = "Series3";
            series3.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series4.ChartArea = "ChartArea4";
            series4.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series4.Legend = "Legend1";
            series4.Name = "Series4";
            series4.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series5.ChartArea = "ChartArea5";
            series5.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series5.Legend = "Legend1";
            series5.Name = "Series5";
            series5.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series6.ChartArea = "ChartArea6";
            series6.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series6.Legend = "Legend1";
            series6.Name = "Series6";
            series6.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series7.ChartArea = "ChartArea7";
            series7.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series7.Legend = "Legend1";
            series7.Name = "Series7";
            series7.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series8.ChartArea = "ChartArea8";
            series8.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series8.Legend = "Legend1";
            series8.Name = "Series8";
            series8.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series9.ChartArea = "ChartArea9";
            series9.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series9.Legend = "Legend1";
            series9.Name = "Series9";
            series9.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series10.ChartArea = "ChartArea10";
            series10.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series10.Legend = "Legend1";
            series10.Name = "Series10";
            series10.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series11.ChartArea = "ChartArea11";
            series11.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series11.Legend = "Legend1";
            series11.Name = "Series11";
            series11.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series12.ChartArea = "ChartArea12";
            series12.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series12.Legend = "Legend1";
            series12.Name = "Series12";
            series12.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series13.ChartArea = "ChartArea13";
            series13.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series13.Legend = "Legend1";
            series13.Name = "Series13";
            series13.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series14.ChartArea = "ChartArea14";
            series14.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series14.Legend = "Legend1";
            series14.Name = "Series14";
            series14.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series15.ChartArea = "ChartArea15";
            series15.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series15.Legend = "Legend1";
            series15.Name = "Series15";
            series15.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series16.ChartArea = "ChartArea16";
            series16.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series16.Legend = "Legend1";
            series16.Name = "Series16";
            series16.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series17.ChartArea = "ChartArea17";
            series17.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series17.Legend = "Legend1";
            series17.Name = "Series17";
            series17.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series18.ChartArea = "ChartArea18";
            series18.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series18.Legend = "Legend1";
            series18.Name = "Series18";
            series18.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series19.ChartArea = "ChartArea19";
            series19.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series19.Legend = "Legend1";
            series19.Name = "Series19";
            series19.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series20.ChartArea = "ChartArea20";
            series20.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series20.Legend = "Legend1";
            series20.Name = "Series20";
            series20.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series21.ChartArea = "ChartArea21";
            series21.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series21.Legend = "Legend1";
            series21.Name = "Series21";
            series21.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series22.ChartArea = "ChartArea22";
            series22.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series22.Legend = "Legend1";
            series22.Name = "Series22";
            series22.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series23.ChartArea = "ChartArea23";
            series23.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series23.Legend = "Legend1";
            series23.Name = "Series23";
            series23.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series24.ChartArea = "ChartArea24";
            series24.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series24.Legend = "Legend1";
            series24.Name = "Series24";
            series24.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series25.ChartArea = "ChartArea25";
            series25.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series25.Legend = "Legend1";
            series25.Name = "Series25";
            series25.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series26.ChartArea = "ChartArea26";
            series26.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series26.Legend = "Legend1";
            series26.Name = "Series26";
            series26.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series26.YValuesPerPoint = 3;
            series27.ChartArea = "ChartArea27";
            series27.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series27.Legend = "Legend1";
            series27.Name = "Series27";
            series27.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series27.YValuesPerPoint = 3;
            series28.ChartArea = "ChartArea28";
            series28.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series28.Legend = "Legend1";
            series28.Name = "Series28";
            series28.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series28.YValuesPerPoint = 3;
            series29.ChartArea = "ChartArea29";
            series29.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series29.Legend = "Legend1";
            series29.Name = "Series29";
            series29.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series29.YValuesPerPoint = 3;
            series30.ChartArea = "ChartArea30";
            series30.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series30.Legend = "Legend1";
            series30.Name = "Series30";
            series30.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series30.YValuesPerPoint = 3;
            series31.ChartArea = "ChartArea31";
            series31.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series31.Legend = "Legend1";
            series31.Name = "Series31";
            series31.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series31.YValuesPerPoint = 3;
            series32.ChartArea = "ChartArea32";
            series32.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series32.Legend = "Legend1";
            series32.Name = "Series32";
            series32.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series32.YValuesPerPoint = 3;
            series33.ChartArea = "ChartArea33";
            series33.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series33.Legend = "Legend1";
            series33.Name = "Series33";
            series33.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series33.YValuesPerPoint = 3;
            this.chart1.Series.Add(series1);
            this.chart1.Series.Add(series2);
            this.chart1.Series.Add(series3);
            this.chart1.Series.Add(series4);
            this.chart1.Series.Add(series5);
            this.chart1.Series.Add(series6);
            this.chart1.Series.Add(series7);
            this.chart1.Series.Add(series8);
            this.chart1.Series.Add(series9);
            this.chart1.Series.Add(series10);
            this.chart1.Series.Add(series11);
            this.chart1.Series.Add(series12);
            this.chart1.Series.Add(series13);
            this.chart1.Series.Add(series14);
            this.chart1.Series.Add(series15);
            this.chart1.Series.Add(series16);
            this.chart1.Series.Add(series17);
            this.chart1.Series.Add(series18);
            this.chart1.Series.Add(series19);
            this.chart1.Series.Add(series20);
            this.chart1.Series.Add(series21);
            this.chart1.Series.Add(series22);
            this.chart1.Series.Add(series23);
            this.chart1.Series.Add(series24);
            this.chart1.Series.Add(series25);
            this.chart1.Series.Add(series26);
            this.chart1.Series.Add(series27);
            this.chart1.Series.Add(series28);
            this.chart1.Series.Add(series29);
            this.chart1.Series.Add(series30);
            this.chart1.Series.Add(series31);
            this.chart1.Series.Add(series32);
            this.chart1.Series.Add(series33);
            this.chart1.Size = new System.Drawing.Size(585, 466);
            this.chart1.TabIndex = 21;
            this.chart1.Text = "chart2";
            // 
            // restart
            // 
            this.restart.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.restart.Location = new System.Drawing.Point(198, 641);
            this.restart.Name = "restart";
            this.restart.Size = new System.Drawing.Size(218, 30);
            this.restart.TabIndex = 22;
            this.restart.Text = "Reprendre à Zéro";
            this.restart.UseVisualStyleBackColor = true;
            this.restart.Click += new System.EventHandler(this.restart_Click);
            // 
            // Fichier
            // 
            this.Fichier.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ouvrirToolStripMenuItem,
            this.enregistrerToolStripMenuItem,
            this.quitterToolStripMenuItem});
            this.Fichier.Name = "Fichier";
            this.Fichier.Size = new System.Drawing.Size(54, 20);
            this.Fichier.Text = "Fichier";
            // 
            // ouvrirToolStripMenuItem
            // 
            this.ouvrirToolStripMenuItem.Name = "ouvrirToolStripMenuItem";
            this.ouvrirToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.ouvrirToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.ouvrirToolStripMenuItem.Text = "Ouvrir";
            this.ouvrirToolStripMenuItem.Click += new System.EventHandler(this.load_Click);
            // 
            // enregistrerToolStripMenuItem
            // 
            this.enregistrerToolStripMenuItem.Name = "enregistrerToolStripMenuItem";
            this.enregistrerToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.enregistrerToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.enregistrerToolStripMenuItem.Text = "Enregistrer";
            this.enregistrerToolStripMenuItem.Click += new System.EventHandler(this.save_Click);
            // 
            // quitterToolStripMenuItem
            // 
            this.quitterToolStripMenuItem.Name = "quitterToolStripMenuItem";
            this.quitterToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
            this.quitterToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.quitterToolStripMenuItem.Text = "Quitter";
            this.quitterToolStripMenuItem.Click += new System.EventHandler(this.quit_Click);
            // 
            // Édition
            // 
            this.Édition.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.eraseAll});
            this.Édition.Name = "Édition";
            this.Édition.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Delete)));
            this.Édition.Size = new System.Drawing.Size(56, 20);
            this.Édition.Text = "Édition";
            // 
            // eraseAll
            // 
            this.eraseAll.Name = "eraseAll";
            this.eraseAll.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Delete)));
            this.eraseAll.Size = new System.Drawing.Size(264, 22);
            this.eraseAll.Text = "Effacer toutes les réponses";
            this.eraseAll.Click += new System.EventHandler(this.eraseAllToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.changerDeQuestionnaireToolStripMenuItem,
            this.about});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(61, 20);
            this.toolStripMenuItem1.Text = "Options";
            // 
            // changerDeQuestionnaireToolStripMenuItem
            // 
            this.changerDeQuestionnaireToolStripMenuItem.Name = "changerDeQuestionnaireToolStripMenuItem";
            this.changerDeQuestionnaireToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.changerDeQuestionnaireToolStripMenuItem.Text = "Changer de questionnaire";
            this.changerDeQuestionnaireToolStripMenuItem.Click += new System.EventHandler(this.changeMCQ_Click);
            // 
            // about
            // 
            this.about.Name = "about";
            this.about.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.about.Size = new System.Drawing.Size(210, 22);
            this.about.Text = "À propos";
            this.about.Click += new System.EventHandler(this.about_Click);
            // 
            // autoSave
            // 
            this.autoSave.Checked = true;
            this.autoSave.CheckOnClick = true;
            this.autoSave.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoSave.Name = "autoSave";
            this.autoSave.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.autoSave.Size = new System.Drawing.Size(340, 22);
            this.autoSave.Text = "Automatiquement sauvegarder à la validation";
            this.autoSave.ToolTipText = "Activer la sauvegarde automatique lorsque vous valider le QCM (à condition d\'avoi" +
                "r préalablement entré le nom du salarié)";
            // 
            // nameBox
            // 
            this.nameBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
            this.nameBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.nameBox.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nameBox.FormattingEnabled = true;
            this.nameBox.Location = new System.Drawing.Point(126, 25);
            this.nameBox.Name = "nameBox";
            this.nameBox.Size = new System.Drawing.Size(200, 24);
            this.nameBox.TabIndex = 26;
            this.nameBox.SelectionChangeCommitted += new System.EventHandler(this.nameBox_Click);
            this.nameBox.Enter += new System.EventHandler(this.nameBox_Dropdown);
            this.nameBox.MouseEnter += new System.EventHandler(this.nameBox_MouseEnter);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.menuStrip1.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.toolStripMenuItem6,
            this.navigationToolStripMenuItem,
            this.toolStripMenuItem8,
            this.saveNewMCQ,
            this.newMCQbox});
            this.menuStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(609, 24);
            this.menuStrip1.TabIndex = 27;
            this.menuStrip1.Text = "main";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem3,
            this.toolStripMenuItem4,
            this.toolStripMenuItem5});
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(53, 20);
            this.toolStripMenuItem2.Text = "Fichier";
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.toolStripMenuItem3.Size = new System.Drawing.Size(173, 22);
            this.toolStripMenuItem3.Text = "Ouvrir";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.load_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.toolStripMenuItem4.Size = new System.Drawing.Size(173, 22);
            this.toolStripMenuItem4.Text = "Enregistrer";
            this.toolStripMenuItem4.Click += new System.EventHandler(this.save_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
            this.toolStripMenuItem5.Size = new System.Drawing.Size(173, 22);
            this.toolStripMenuItem5.Text = "Quitter";
            this.toolStripMenuItem5.Click += new System.EventHandler(this.quit_Click);
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem7,
            this.insérerLaDateToolStripMenuItem});
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Delete)));
            this.toolStripMenuItem6.Size = new System.Drawing.Size(56, 20);
            this.toolStripMenuItem6.Text = "Édition";
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            this.toolStripMenuItem7.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Delete)));
            this.toolStripMenuItem7.Size = new System.Drawing.Size(273, 22);
            this.toolStripMenuItem7.Text = "Effacer toutes les réponses";
            this.toolStripMenuItem7.Click += new System.EventHandler(this.eraseAllToolStripMenuItem_Click);
            // 
            // insérerLaDateToolStripMenuItem
            // 
            this.insérerLaDateToolStripMenuItem.Name = "insérerLaDateToolStripMenuItem";
            this.insérerLaDateToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Insert)));
            this.insérerLaDateToolStripMenuItem.Size = new System.Drawing.Size(273, 22);
            this.insérerLaDateToolStripMenuItem.Text = "Insérer la date";
            this.insérerLaDateToolStripMenuItem.Click += new System.EventHandler(this.insérerLaDateToolStripMenuItem_Click);
            // 
            // navigationToolStripMenuItem
            // 
            this.navigationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.navigNext,
            this.navigPrevious,
            this.navigEnd});
            this.navigationToolStripMenuItem.Name = "navigationToolStripMenuItem";
            this.navigationToolStripMenuItem.Size = new System.Drawing.Size(75, 20);
            this.navigationToolStripMenuItem.Text = "Navigation";
            // 
            // navigNext
            // 
            this.navigNext.Name = "navigNext";
            this.navigNext.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.S)));
            this.navigNext.Size = new System.Drawing.Size(173, 22);
            this.navigNext.Text = "Suivant";
            this.navigNext.Click += new System.EventHandler(this.next_Click);
            // 
            // navigPrevious
            // 
            this.navigPrevious.Name = "navigPrevious";
            this.navigPrevious.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.E)));
            this.navigPrevious.Size = new System.Drawing.Size(173, 22);
            this.navigPrevious.Text = "Précédent";
            this.navigPrevious.Click += new System.EventHandler(this.previous_Click);
            // 
            // navigEnd
            // 
            this.navigEnd.Name = "navigEnd";
            this.navigEnd.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Space)));
            this.navigEnd.Size = new System.Drawing.Size(173, 22);
            this.navigEnd.Text = "Valider";
            this.navigEnd.Click += new System.EventHandler(this.end_Click);
            // 
            // toolStripMenuItem8
            // 
            this.toolStripMenuItem8.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.autoSave,
            this.toolStripMenuItem10,
            this.newMCQ,
            this.toolStripMenuItem11});
            this.toolStripMenuItem8.Name = "toolStripMenuItem8";
            this.toolStripMenuItem8.Size = new System.Drawing.Size(61, 20);
            this.toolStripMenuItem8.Text = "Options";
            // 
            // toolStripMenuItem10
            // 
            this.toolStripMenuItem10.Name = "toolStripMenuItem10";
            this.toolStripMenuItem10.Size = new System.Drawing.Size(340, 22);
            this.toolStripMenuItem10.Text = "Changer de questionnaire";
            this.toolStripMenuItem10.Click += new System.EventHandler(this.changeMCQ_Click);
            // 
            // newMCQ
            // 
            this.newMCQ.Name = "newMCQ";
            this.newMCQ.Size = new System.Drawing.Size(340, 22);
            this.newMCQ.Text = "Créer un nouveau questionnaire";
            this.newMCQ.Click += new System.EventHandler(this.CreateMCQ_Click);
            // 
            // toolStripMenuItem11
            // 
            this.toolStripMenuItem11.Name = "toolStripMenuItem11";
            this.toolStripMenuItem11.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.toolStripMenuItem11.Size = new System.Drawing.Size(340, 22);
            this.toolStripMenuItem11.Text = "À propos";
            this.toolStripMenuItem11.Click += new System.EventHandler(this.about_Click);
            // 
            // saveNewMCQ
            // 
            this.saveNewMCQ.Name = "saveNewMCQ";
            this.saveNewMCQ.Size = new System.Drawing.Size(129, 20);
            this.saveNewMCQ.Text = "Sauvegarder le QCM";
            this.saveNewMCQ.Visible = false;
            this.saveNewMCQ.Click += new System.EventHandler(this.CreateMCQ_Click);
            // 
            // newMCQbox
            // 
            this.newMCQbox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.newMCQbox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.newMCQbox.Name = "newMCQbox";
            this.newMCQbox.Size = new System.Drawing.Size(100, 23);
            this.newMCQbox.Text = "Nom du QCM";
            this.newMCQbox.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.newMCQbox.Visible = false;
            this.newMCQbox.Click += new System.EventHandler(this.newMCQbox_Click);
            // 
            // notesBox
            // 
            this.notesBox.Location = new System.Drawing.Point(12, 560);
            this.notesBox.Name = "notesBox";
            this.notesBox.Size = new System.Drawing.Size(585, 75);
            this.notesBox.TabIndex = 28;
            this.notesBox.Text = "";
            // 
            // notes
            // 
            this.notes.AutoSize = true;
            this.notes.Location = new System.Drawing.Point(9, 544);
            this.notes.Name = "notes";
            this.notes.Size = new System.Drawing.Size(120, 13);
            this.notes.TabIndex = 29;
            this.notes.Text = "Notes supplémentaires :";
            // 
            // saveChart
            // 
            this.saveChart.BackColor = System.Drawing.Color.Transparent;
            this.saveChart.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.saveChart.FlatAppearance.BorderSize = 0;
            this.saveChart.FlatAppearance.CheckedBackColor = System.Drawing.Color.Transparent;
            this.saveChart.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.saveChart.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightGray;
            this.saveChart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.saveChart.ForeColor = System.Drawing.Color.Transparent;
            this.saveChart.Image = global::MCQHelper.Properties.Resources.document_save;
            this.saveChart.Location = new System.Drawing.Point(563, 254);
            this.saveChart.Name = "saveChart";
            this.saveChart.Size = new System.Drawing.Size(32, 32);
            this.saveChart.TabIndex = 32;
            this.saveChart.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.saveChart.UseVisualStyleBackColor = false;
            this.saveChart.Click += new System.EventHandler(this.saveChart_Click);
            // 
            // showLegend
            // 
            this.showLegend.BackColor = System.Drawing.Color.Transparent;
            this.showLegend.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.showLegend.FlatAppearance.BorderSize = 0;
            this.showLegend.FlatAppearance.CheckedBackColor = System.Drawing.Color.Transparent;
            this.showLegend.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.showLegend.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightGray;
            this.showLegend.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.showLegend.ForeColor = System.Drawing.Color.Transparent;
            this.showLegend.Image = global::MCQHelper.Properties.Resources.medical_invoice_information;
            this.showLegend.Location = new System.Drawing.Point(563, 292);
            this.showLegend.Name = "showLegend";
            this.showLegend.Size = new System.Drawing.Size(32, 32);
            this.showLegend.TabIndex = 30;
            this.showLegend.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.showLegend.UseVisualStyleBackColor = false;
            this.showLegend.Click += new System.EventHandler(this.showLegend_Click);
            // 
            // zoomChart
            // 
            this.zoomChart.BackColor = System.Drawing.Color.Transparent;
            this.zoomChart.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.zoomChart.FlatAppearance.BorderSize = 0;
            this.zoomChart.FlatAppearance.CheckedBackColor = System.Drawing.Color.Transparent;
            this.zoomChart.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.zoomChart.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightGray;
            this.zoomChart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.zoomChart.ForeColor = System.Drawing.Color.Transparent;
            this.zoomChart.Image = global::MCQHelper.Properties.Resources.search;
            this.zoomChart.Location = new System.Drawing.Point(563, 330);
            this.zoomChart.Name = "zoomChart";
            this.zoomChart.Size = new System.Drawing.Size(32, 32);
            this.zoomChart.TabIndex = 31;
            this.zoomChart.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.zoomChart.UseVisualStyleBackColor = false;
            this.zoomChart.Click += new System.EventHandler(this.zoom_Click);
            // 
            // end
            // 
            this.end.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.end.Image = global::MCQHelper.Properties.Resources.Check_icon1;
            this.end.Location = new System.Drawing.Point(283, 624);
            this.end.Name = "end";
            this.end.Size = new System.Drawing.Size(48, 48);
            this.end.TabIndex = 19;
            this.end.UseVisualStyleBackColor = true;
            this.end.Click += new System.EventHandler(this.end_Click);
            // 
            // creationBox
            // 
            this.creationBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.creationBox.Location = new System.Drawing.Point(0, 0);
            this.creationBox.Name = "creationBox";
            this.creationBox.Size = new System.Drawing.Size(609, 675);
            this.creationBox.TabIndex = 33;
            this.creationBox.Text = "";
            this.creationBox.Visible = false;
            // 
            // Form1
            // 
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(609, 675);
            this.Controls.Add(this.chart1);
            this.Controls.Add(this.saveChart);
            this.Controls.Add(this.showLegend);
            this.Controls.Add(this.zoomChart);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.nameBox);
            this.Controls.Add(this.end);
            this.Controls.Add(this.next);
            this.Controls.Add(this.previous);
            this.Controls.Add(this.checkedListBox5);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.checkedListBox4);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.checkedListBox3);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.checkedListBox2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.checkedListBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.date);
            this.Controls.Add(this.load);
            this.Controls.Add(this.save);
            this.Controls.Add(this.labelname);
            this.Controls.Add(this.restart);
            this.Controls.Add(this.notes);
            this.Controls.Add(this.notesBox);
            this.Controls.Add(this.creationBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Opacity = 0.96D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Assistant au QCM";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void nameBox_MouseEnter(object sender, EventArgs e)
        {
            nameBox_Dropdown(sender, e);
        }

    }
}
