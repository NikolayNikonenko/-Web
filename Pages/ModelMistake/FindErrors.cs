using ASTRALib;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using перенос_бд_на_Web.Models;
using перенос_бд_на_Web.Pages.TM;

namespace перенос_бд_на_Web.Pages.ModelMistake
{
    public class FindErrors : PageModel
    {
        private readonly ApplicationContext _modelContext;

        public List<ModelErrors> ErrModel { get; set; } = new List<ModelErrors>();

        public FindErrors(ApplicationContext modelContext)
        {
            _modelContext = modelContext;
        }
        public async Task OnGetAsync(string selectedFilePath)
        {
            string dirName = selectedFilePath;
            int totalSteps = 100; // общее количество шагов в процессе
            Action<int> reportProgress = step =>
            {
                // Здесь обновите статус бар или передайте данные в UI через SignalR
                Console.WriteLine($"Шаг {step} из {totalSteps}");
            };

            await CalculateModelErrorsAsync(dirName, totalSteps, reportProgress);
            ErrModel = await _modelContext.modelErrors.AsNoTracking().ToListAsync();
        }
        public async Task CalculateModelErrorsAsync(string dirName, int totalSteps, Action<int> reportProgress)
        {
            // Выполнение части работы
            await Task.Delay(100); // Симуляция асинхронной операции

            var directory = new DirectoryInfo(dirName);
            Console.BufferHeight = 10000;

            string PathFile = ($"{dirName}");
            // Создаем указатель на экземпляр RastrWin и его запуск
            IRastr Rastr = new Rastr();
            // Загружаем файл с режимом
            Rastr.Load(RG_KOD.RG_REPL, PathFile, "");
            // Обращение к таблице ТИ:каналы
            ITable _tableTIChannel = (ITable)Rastr.Tables.Item("ti");
            // Обращение к колонке № ТМ
            ICol _numberTM = (ICol)_tableTIChannel.Cols.Item("Num");
            // Обращение к колонке значений
            ICol _znachTm = (ICol)_tableTIChannel.Cols.Item("ti_val");
            // Обращение к колонке тип
            ICol tip = (ICol)_tableTIChannel.Cols.Item("type");
            // Обращение к колонке id1
            ICol _id1 = (ICol)_tableTIChannel.Cols.Item("id1");
            // Обращение к колонке id2
            ICol _id2 = (ICol)_tableTIChannel.Cols.Item("id2");
            // Обращение к колонке id3
            ICol _id3 = (ICol)_tableTIChannel.Cols.Item("id3");
            // Обращение к коду в ОС
            ICol _cod = (ICol)_tableTIChannel.Cols.Item("cod_oc");
            // Обращение к типу привязки
            ICol _privyazka = (ICol)_tableTIChannel.Cols.Item("prv_num");
            // Обращение к типу телеметрии
            ICol _typeTM = (ICol)_tableTIChannel.Cols.Item("type");

            // Обращение к таблице ветви
            ITable _table_vetvi = (ITable)Rastr.Tables.Item("vetv");
            // Обращение к колонке N_нач
            ICol N_nach = (ICol)_table_vetvi.Cols.Item("ip");
            // Обращение к колонке N_кон
            ICol N_kon = (ICol)_table_vetvi.Cols.Item("iq");
            // Обращение к колонке R линии
            ICol rLin = (ICol)_table_vetvi.Cols.Item("r");
            // Обращение к колонке X линии
            ICol xLin = (ICol)_table_vetvi.Cols.Item("x");
            // Обращение к колонке тип ветви
            ICol tip_vetvi = (ICol)_table_vetvi.Cols.Item("tip");

            int schet = _tableTIChannel.Size;
            double tmp;
            string tmp1;
            double indexTM;
            double izm_TM;
            double id1TM;
            double id2TM;
            double id3TM;

            double[] indeksi = new double[schet];
            int[] tipTM = new int[schet];
            double[,] index_TM_InRastr = new double[schet, 5];
            Console.BufferHeight = 20000;
            for (int numTM = 0; numTM < schet; numTM++)
            {
                izm_TM = (double)_znachTm.get_ZN(numTM);
                indexTM = (double)_numberTM.get_ZN(numTM);
                id1TM = (int)_id1.get_ZN((int)numTM);
                id2TM = (int)_id2.get_ZN((int)numTM);
                id3TM = (int)_id3.get_ZN((int)numTM);

                index_TM_InRastr[numTM, 0] = indexTM;
                index_TM_InRastr[numTM, 1] = id1TM;
                index_TM_InRastr[numTM, 2] = id2TM;
                index_TM_InRastr[numTM, 3] = id3TM;
                index_TM_InRastr[numTM, 4] = Convert.ToDouble(numTM);

                tipTM[numTM] = (int)tip.get_ZN(numTM);
                indeksi[numTM] = indexTM;
            }

            List<int> tipsDubl = new List<int>();
            int typeOfDubl;
            var seen = new HashSet<double>();
            var dublicates = new HashSet<double>();
            int counter = 0;
            foreach (var item in indeksi)
            {
                if (!(seen.Add(item)) && (item != 0))
                {
                    typeOfDubl = (int)tip.get_ZN((int)index_TM_InRastr[counter, 4]);
                    dublicates.Add(item);
                    tipsDubl.Add(typeOfDubl);
                }
                counter++;
            }
            double[] povtori = new double[dublicates.Count];
            int[] countPov = new int[dublicates.Count];
            Console.WriteLine("Дубли");
            for (int c = 0; c < dublicates.Count; c++)
            {
                double target = dublicates.ElementAt(c);
                int countTarget = indeksi.Count(item => item == target);
                povtori[c] = (int)target;
                countPov[c] = countTarget;
                Console.WriteLine($"{povtori[c]}\t {countPov[c]}");
            }

            totalSteps = povtori.Length;
            int currentStep = 0;

            var db = _modelContext;

            db.Database.ExecuteSqlRaw("DELETE FROM \"modelErrors\";");

            for (int w = 0; w < povtori.Length; w++)
            {
                List<double> strokiDubley = BinarySearch(index_TM_InRastr, povtori[w], countPov[w]);
                List<double> TIiTS = new List<double>();
                List<string> privMassiv = new List<string>();


                bool flagBrek = false;
                bool flagTS = false;
                for (int c = 0; c < strokiDubley.Count; c++)
                {
                    int strokaInMassiv = (int)strokiDubley[c];
                    int numbeNode = (int)index_TM_InRastr[strokaInMassiv, 1];
                    int tipVetkaNach;
                    int tipVetkaKon;

                    string priv = (string)_privyazka.get_ZN((int)index_TM_InRastr[strokaInMassiv, 4]);
                    if (flagBrek)
                    {
                        break;
                    }
                    for (int z = 0; z < strokiDubley.Count; z++)
                    {
                        privMassiv.Add((string)_privyazka.get_ZN((int)index_TM_InRastr[(int)strokiDubley[z], 4]));
                        TIiTS.Add((int)_typeTM.get_ZN((int)index_TM_InRastr[(int)strokiDubley[z], 4]));
                    }

                    if (privMassiv.Contains("U"))
                    {
                        _table_vetvi.SetSel($"ip = {numbeNode}");
                        int n = _table_vetvi.FindNextSel[-1];

                        while (true)
                        {
                            if ((n == -1))
                            {
                                break;
                            }

                            tipVetkaNach = (int)tip_vetvi.get_ZN(n);

                            if (!GetMistakeTM(strokiDubley, index_TM_InRastr, Rastr))
                            {

                                Console.WriteLine($"не достоверна ТМ № {index_TM_InRastr[(int)strokiDubley[0], 0]}");
                                VstavkaInPostgres(index_TM_InRastr, strokiDubley, "ТМ с разных объектов");
                                flagBrek = true;
                            }

                            n = _table_vetvi.FindNextSel[n];
                            if (flagBrek)
                            {
                                break;
                            }

                        }

                        if (flagBrek)
                        {
                            break;
                        }

                        _table_vetvi.SetSel($"iq = {numbeNode}");
                        int q = _table_vetvi.FindNextSel[-1];
                        if (!flagBrek)
                        {
                            while (true)
                            {
                                if (q == -1)
                                {
                                    break;
                                }

                                tipVetkaKon = (int)tip_vetvi.get_ZN(q);


                                if (!GetMistakeTM(strokiDubley, index_TM_InRastr, Rastr))
                                {

                                    Console.WriteLine($"не достоверна ТМ № {index_TM_InRastr[(int)strokiDubley[0], 0]}");
                                    VstavkaInPostgres(index_TM_InRastr, strokiDubley, "ТМ с разных объектов");
                                    flagBrek = true;
                                }

                                q = _table_vetvi.FindNextSel[q];
                                if (flagBrek)
                                {
                                    break;
                                }
                            }
                            if (flagBrek)
                            {
                                break;
                            }
                        }
                    }
                    bool nach1kon2 = index_TM_InRastr[(int)strokiDubley[0], 1] == index_TM_InRastr[(int)strokiDubley[1], 2];
                    bool nach2kon1 = index_TM_InRastr[(int)strokiDubley[1], 1] == index_TM_InRastr[(int)strokiDubley[0], 2];
                    bool nach1nach2 = index_TM_InRastr[(int)strokiDubley[0], 1] == index_TM_InRastr[(int)strokiDubley[1], 1];
                    bool kon1kon2 = index_TM_InRastr[(int)strokiDubley[1], 2] == index_TM_InRastr[(int)strokiDubley[0], 2];


                    switch (priv)
                    {
                        case "Pкон":
                        case "Pнач":
                        case "Qкон":
                        case "Qнач":
                            {

                                if ((nach1kon2 || nach2kon1) || (nach1nach2 && !kon1kon2) || (!nach1nach2 && kon1kon2))
                                {

                                    if (GetErrPosled(strokiDubley, index_TM_InRastr, Rastr))
                                    {
                                        flagBrek = true;
                                        if ((TIiTS.IndexOf(1) != -1) || (TIiTS.IndexOf(3) != -1))
                                        {
                                            break;
                                        }
                                        VstavkaInPostgres(index_TM_InRastr, strokiDubley, "поcледовательные ТМ");
                                    }
                                    Console.WriteLine($"{index_TM_InRastr[(int)strokiDubley[0], 0]} ТМ не последовательны");

                                }

                                for (int i = 1; i < strokiDubley.Count; i++)
                                {

                                    int nach1 = (int)index_TM_InRastr[(int)strokiDubley[0], 1];
                                    int kon1 = (int)index_TM_InRastr[(int)strokiDubley[0], 2];
                                    int nachN = (int)index_TM_InRastr[(int)strokiDubley[i], 1];
                                    int konN = (int)index_TM_InRastr[(int)strokiDubley[i], 2];

                                    if ((nach1 != nachN) || (kon1 != konN))
                                    {
                                        if (!GetMistakeTM(strokiDubley, index_TM_InRastr, Rastr))
                                        {
                                            if ((TIiTS.IndexOf(1) != -1) || (TIiTS.IndexOf(3) != -1))
                                            {
                                                break;
                                            }
                                            Console.WriteLine($"ТМ {index_TM_InRastr[(int)strokiDubley[0], 0]} не параллельна");
                                            VstavkaInPostgres(index_TM_InRastr, strokiDubley, "ТМ не параллельны");
                                            flagBrek = true;
                                            break;
                                        }
                                    }

                                }

                                if (GetErrZnak(strokiDubley, index_TM_InRastr, Rastr))
                                {
                                    Console.WriteLine($"Перепутаны знаки перетоков в начале " +
                                        $"или конце ЛЭП для ТМ {index_TM_InRastr[(int)strokiDubley[0], 0]}");
                                    VstavkaInPostgres(index_TM_InRastr, strokiDubley, "Перепутаны знаки перетоков");
                                    flagBrek = true;
                                    break;
                                }
                                break;

                            }
                            break;
                    }
                }
                currentStep++;
                int progress = (currentStep * 100) / totalSteps;
                reportProgress(progress);
            }
            
        }

        public static bool GetErrPosled(List<double> nKontsov, double[,] Massiv, IRastr Rastr)
        {
            // Обращение к таблице ветви
            ITable _table_vetvi = (ITable)Rastr.Tables.Item("vetv");
            // Обращение к колонке N_нач
            ICol N_nach = (ICol)_table_vetvi.Cols.Item("ip");
            // Обращение к колонке N_кон
            ICol N_kon = (ICol)_table_vetvi.Cols.Item("iq");
            // Обращение к колонке тип ветви
            ICol tip_vetvi = (ICol)_table_vetvi.Cols.Item("tip");
            // Обращение к таблице узлы
            ITable node = (ITable)Rastr.Tables.Item("node");
            // Обращение к колонке Pn
            ICol Pn = (ICol)node.Cols.Item("pn");
            // Обращение к колонке Qn
            ICol Qn = (ICol)node.Cols.Item("qn");
            // Обращение к колонке Pg
            ICol Pg = (ICol)node.Cols.Item("pg");
            // Обращение к колонке Qg
            ICol Qg = (ICol)node.Cols.Item("qg");
            int obshiiuzel = 0;
            List<double> havingNodes = new List<double>{ Massiv[(int)nKontsov[0], 1],
                                                     Massiv[(int)nKontsov[0], 2],
                                                     Massiv[(int)nKontsov[1], 1],
                                                     Massiv[(int)nKontsov[1], 2]};
            if (Massiv[(int)nKontsov[0], 1] == Massiv[(int)nKontsov[1], 2])
            {
                obshiiuzel = (int)Massiv[(int)nKontsov[0], 1];
            }
            if (Massiv[(int)nKontsov[1], 1] == Massiv[(int)nKontsov[0], 2])
            {
                obshiiuzel = (int)Massiv[(int)nKontsov[1], 1];
            }
            node.SetSel($"ny = {obshiiuzel}");
            int n = _table_vetvi.FindNextSel[-1];
            while (n != -1)
            {
                if (((double)Pn.get_ZN(n) != 0) ||
                    ((double)Qn.get_ZN(n) != 0) ||
                    ((double)Pg.get_ZN(n) != 0) ||
                    ((double)Qg.get_ZN(n) != 0))
                {
                    return true;
                }
                n = _table_vetvi.FindNextSel[n];
            }
            _table_vetvi.SetSel($"ip = {obshiiuzel}");
            n = _table_vetvi.FindNextSel[-1];
            while (n != -1)
            {

                if (havingNodes.IndexOf((int)N_kon.get_ZN(n)) == -1)
                {
                    return true;
                }
                n = _table_vetvi.FindNextSel[n];
            }
            return false;
        }
        public static bool GetErrZnak(List<double> nKontsov, double[,] Massiv, IRastr Rastr)
        {
            ITable _tableTIChannel = (ITable)Rastr.Tables.Item("ti");
            ICol _privyazka = (ICol)_tableTIChannel.Cols.Item("prv_num");
            // Обращение к колонке значений
            ICol _znachTm = (ICol)_tableTIChannel.Cols.Item("ti_val");
            List<double> nachNodesP = new List<double>();
            List<double> konNodesP = new List<double>();
            List<double> nachNodesQ = new List<double>();
            List<double> konNodesQ = new List<double>();
            bool flagP = false;
            bool flagQ = false;
            for (int i = 0; i < nKontsov.Count; i++)
            {
                string priv = (string)_privyazka.get_ZN((int)Massiv[(int)nKontsov[i], 4]);

                switch (priv)
                {
                    case "Pкон":
                        {
                            konNodesP.Add((double)_znachTm.get_ZN(i));
                            flagP = true;
                            break;
                        }
                    case "Pнач":
                        {
                            nachNodesP.Add((double)_znachTm.get_ZN(i));
                            break;
                        }
                    case "Qкон":
                        {
                            konNodesQ.Add((double)_znachTm.get_ZN(i));
                            flagQ = true;
                            break;
                        }
                    case "Qнач":
                        {
                            nachNodesQ.Add((double)_znachTm.get_ZN(i));
                            break;
                        }
                }

            }
            if (flagP)
            {
                for (int j = 0; j < nachNodesP.Count; j++)
                {
                    for (int k = 0; k < konNodesP.Count; k++)
                    {
                        if (((konNodesP[k] > 0) && (nachNodesP[j] > 0))
                            || ((konNodesP[k] < 0) && (nachNodesP[j] < 0)))
                        {
                            return true;
                        }
                    }
                }
            }
            if (flagQ)
            {
                for (int j = 0; j < nachNodesQ.Count; j++)
                {
                    for (int k = 0; k < konNodesQ.Count; k++)
                    {
                        if (((konNodesQ[k] > 0) && (nachNodesQ[j] > 0))
                            || ((konNodesQ[k] < 0) && (nachNodesQ[j] < 0)))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void VstavkaInPostgres(double[,] index_TM_InRastr, List<double> strokiDubley, string tip)
        {
            var db = _modelContext;

                var typeMistakesInModel = new ModelErrors
                {
                    ID = Guid.NewGuid(),
                    IndexTm = index_TM_InRastr[(int)strokiDubley[0], 0],
                    ErrorType = tip
                };
                db.Add(typeMistakesInModel);

                try
                {
                    Console.WriteLine("Сохранение изменений в PostgreSql");
                    db.SaveChanges();
                    Console.WriteLine("Изменения успешно сохранены");
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine("Произошла ошибка при изменении данных в БД");
                    Console.WriteLine(ex.InnerException?.Message);
                }

        }

        public static bool GetMistakeTM(List<double> nKontsov, double[,] Massiv, IRastr Rastr)
        {
            // Обращение к таблице ветви
            ITable _table_vetvi = (ITable)Rastr.Tables.Item("vetv");
            // Обращение к колонке N_нач
            ICol N_nach = (ICol)_table_vetvi.Cols.Item("ip");
            // Обращение к колонке N_кон
            ICol N_kon = (ICol)_table_vetvi.Cols.Item("iq");
            // Обращение к колонке R линии
            ICol rLin = (ICol)_table_vetvi.Cols.Item("r");
            // Обращение к колонке X линии
            ICol xLin = (ICol)_table_vetvi.Cols.Item("x");
            // Обращение к колонке тип ветви
            ICol tip_vetvi = (ICol)_table_vetvi.Cols.Item("tip");

            List<double> arrNachVetv = new List<double>();
            double buffnachN = nKontsov[0];
            _table_vetvi.SetSel($"ip = {(int)(Massiv[(int)buffnachN, 1])}");
            int n = _table_vetvi.FindNextSel[-1];
            int counter = 0;
            while (n != -1)
            {
                int buffuzel = (int)N_kon.get_ZN(n);
                if ((int)tip_vetvi.get_ZN(n) == 2)
                {
                    arrNachVetv.Add(buffuzel);

                }
                n = _table_vetvi.FindNextSel[n];
            }
            _table_vetvi.SetSel($"iq = {(int)(Massiv[(int)buffnachN, 1])}");
            n = _table_vetvi.FindNextSel[-1];
            while (n != -1)
            {
                int buffuzel = (int)N_nach.get_ZN(n);
                if ((int)tip_vetvi.get_ZN(n) == 2)
                {
                    arrNachVetv.Add(buffuzel);

                }
                n = _table_vetvi.FindNextSel[n];
            }
            for (int i = 0; i < 2; i++)
            {
                GetNodes(n, ref arrNachVetv, Rastr, 0);
                GetNodes(n, ref arrNachVetv, Rastr, 1);
            }
            for (int i = 0; i < nKontsov.Count; i++)
            {
                for (int j = 0; j < arrNachVetv.Count; j++)
                {
                    if (Massiv[(int)nKontsov[i], 1] == arrNachVetv[j])
                    {
                        counter++;
                    }

                }
            }
            if (counter == nKontsov.Count)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void GetNodes(int n, ref List<double> arrNachVetv, IRastr Rastr, int status)
        {
            // Обращение к таблице ветви
            ITable _table_vetvi = (ITable)Rastr.Tables.Item("vetv");
            // Обращение к колонке N_кон
            ICol N_kon = (ICol)_table_vetvi.Cols.Item("iq");
            // Обращение к колонке тип ветви
            ICol tip_vetvi = (ICol)_table_vetvi.Cols.Item("tip");
            // Обращение к колонке N_нач
            ICol N_nach = (ICol)_table_vetvi.Cols.Item("ip");
            if (status == 0)
            {
                for (int i = 0; i < arrNachVetv.Count; i++)
                {
                    _table_vetvi.SetSel($"ip = {(int)(arrNachVetv[i])}");
                    n = _table_vetvi.FindNextSel[-1];
                    while (n != -1)
                    {
                        int buffuzel = (int)N_kon.get_ZN(n);
                        if (((int)tip_vetvi.get_ZN(n) == 2) && (arrNachVetv.IndexOf(buffuzel) == -1))
                        {
                            arrNachVetv.Add(buffuzel);

                        }
                        n = _table_vetvi.FindNextSel[n];
                    }
                }
            }
            if (status == 1)
            {
                for (int i = 0; i < arrNachVetv.Count; i++)
                {
                    _table_vetvi.SetSel($"iq = {(int)(arrNachVetv[i])}");
                    n = _table_vetvi.FindNextSel[-1];
                    while (n != -1)
                    {
                        int buffuzel = (int)N_nach.get_ZN(n);
                        if (((int)tip_vetvi.get_ZN(n) == 2) && (arrNachVetv.IndexOf(buffuzel) == -1))
                        {
                            arrNachVetv.Add(buffuzel);

                        }
                        n = _table_vetvi.FindNextSel[n];
                    }
                }
            }
        }
        public static List<double> BinarySearch(double[,] index_TM_InRastr, double findThis, int countPow)
        {

            double[,] buffArr = new double[index_TM_InRastr.GetLength(0), 5];
            buffArr = Sort(index_TM_InRastr);
            int low = 0;
            int high = index_TM_InRastr.GetLength(0) - 1;
            int guess = 0;
            double schetschic = 0;
            int schet = index_TM_InRastr.GetLength(0);

            while (buffArr[guess, 0] != findThis)
            {
                guess = (low + high) / 2;
                if (findThis < buffArr[guess, 0])
                {
                    high = guess - 1;
                }
                else if (findThis > buffArr[guess, 0])
                {
                    low = guess + 1;
                }
                if (schetschic > (Math.Log(Convert.ToDouble(schet), 2.0)) + 1)
                {
                    break;
                }
                schetschic++;

            }
            while (true)
            {
                if (buffArr[guess + 1, 0] == buffArr[guess, 0])
                {
                    guess++;
                    continue;
                }
                else
                {
                    break;
                }
            }
            List<double> dublesAndIndexes = new List<double>();
            for (int m = 0; m < countPow; m++)
            {
                dublesAndIndexes.Add((double)guess);
                guess--;
            }
            return dublesAndIndexes;

        }
        public static double[,] Sort(double[,] Massiv)
        {
            for (int j = 0; j < Massiv.GetLength(0); j++)
            {
                for (int k = 0; k < (Massiv.GetLength(0)) - 1 - j; k++)
                {
                    if (Massiv[k, 0] > Massiv[k + 1, 0])
                    {
                        double min1 = Massiv[k, 0];
                        double min2 = Massiv[k, 1];
                        double min3 = Massiv[k, 2];
                        double min4 = Massiv[k, 3];
                        double min5 = Massiv[k, 4];
                        Massiv[k, 0] = Massiv[k + 1, 0];
                        Massiv[k, 1] = Massiv[k + 1, 1];
                        Massiv[k, 2] = Massiv[k + 1, 2];
                        Massiv[k, 3] = Massiv[k + 1, 3];
                        Massiv[k, 4] = Massiv[k + 1, 4];
                        Massiv[k + 1, 0] = min1;
                        Massiv[k + 1, 1] = min2;
                        Massiv[k + 1, 2] = min3;
                        Massiv[k + 1, 3] = min4;
                        Massiv[k + 1, 4] = min5;
                    }
                }
            }
            return Massiv;
        }

    }
}
