using ASTRALib;
using System.Text.RegularExpressions;

namespace перенос_бд_на_Web.Pages.Preprocessing
{
    public class CalculationPTI
    {
        static void Main(string[] args)
        {
            string dirName = "D:\\учеба\\магистратура\\диплом\\правильные срезы\\до корр точно сотка";
            var directory = new DirectoryInfo(dirName);
            DirectoryInfo[] dirs = directory.GetDirectories();
            foreach (DirectoryInfo dir in dirs)
            {
                Regex regex = new Regex(@"\b(\d\d_\d\d_\d\d)");

                Match match1 = regex.Match(Convert.ToString(dir));
                foreach (FileInfo pathFile in dir.GetFiles("roc_debug_after_OC*"))
                {

                    Console.WriteLine(pathFile);
                    Console.WriteLine(dir);

                    string PathFile = ($"{pathFile}");
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
                    // Обращение к коду в ОС
                    // Обращение к колонке id2
                    ICol _id2 = (ICol)_tableTIChannel.Cols.Item("id2");
                    // Обращение к коду в ОС
                    ICol _cod = (ICol)_tableTIChannel.Cols.Item("cod_oc");
                    // Обращение к колонке привязка
                    ICol _privyazka = (ICol)_tableTIChannel.Cols.Item("prv_num");


                    // Обращение к таблице узлы
                    ITable _nodes = (ITable)Rastr.Tables.Item("node");
                    // Обращение к колонке номер узла
                    ICol _numberNode = (ICol)_nodes.Cols.Item("ny");
                    // Обращение к колонке Pн
                    ICol Pn = (ICol)_nodes.Cols.Item("pn");
                    // Обращение к колонке Qн
                    ICol Qn = (ICol)_nodes.Cols.Item("qn");
                    // Обращение к колонке Pг
                    ICol Pg = (ICol)_nodes.Cols.Item("pg");
                    // Обращение к колонке Pг
                    ICol Qg = (ICol)_nodes.Cols.Item("qg");
                    // Обращение к колонке Pн есть
                    ICol Pn_exist = (ICol)_nodes.Cols.Item("exist_load");
                    // Обращение к колонке Pг есть
                    ICol Pg_exist = (ICol)_nodes.Cols.Item("exist_gen");
                    // Обращение к колонке статус узла
                    ICol statusNode = (ICol)_nodes.Cols.Item("sta");
                    // Обращение к колонке тип узла
                    ICol _tipUzla = (ICol)_nodes.Cols.Item("tip");

                    // Обращение к таблице Генераторы УР
                    ITable _generatori = (ITable)Rastr.Tables.Item("Generator");
                    ICol NumberUzelInGenerators = (ICol)_generatori.Cols.Item("Node");

                    // Обращение к таблице реакторы
                    ITable _reactors = (ITable)Rastr.Tables.Item("Reactors");
                    // обращение к колонке номер узла в реакторах


                    int schetUzli = _nodes.Size;
                    double numberNode;
                    bool sta;
                    double pg_est;
                    double pn_est;
                    double pn;
                    double qn;
                    double pg;
                    double qg;
                    string priv;
                    double znachTMPNach;
                    double znachTMQNach;
                    double znachTMPKon;
                    double znachTMQKon;
                    double znachTMPNachForGen;
                    double znachTMQNachForGen;
                    double znachTMPKonForGen;
                    double znachTMQKonForGen;
                    int tipUzla;
                    double znachPNagInUzel;
                    double znachQNagInUzel;
                    List<double> numbersNag = new List<double>();
                    List<double> numbersGen = new List<double>();
                    List<double> znachPNagrForGenPTI = new List<double>();
                    List<double> znachQNagrForGenPTI = new List<double>();


                    for (int numTM = 0; numTM < schetUzli; numTM++)
                    {
                        sta = (bool)statusNode.get_ZN(numTM);
                        numberNode = (int)_numberNode.get_ZN(numTM);
                        pg_est = (int)Pg_exist.get_ZN(numTM);
                        pn_est = (int)Pn_exist.get_ZN(numTM);
                        pn = (double)Pn.get_ZN(numTM);
                        qn = (double)Qn.get_ZN(numTM);
                        pg = (double)Pg.get_ZN(numTM);
                        qg = (double)Qg.get_ZN(numTM);
                        tipUzla = (int)_tipUzla.get_ZN(numTM);
                        znachPNagInUzel = (double)Pn.get_ZN(numTM);
                        znachQNagInUzel = (double)Qn.get_ZN(numTM);


                        if ((sta != true) && (pn_est == 0) && ((pn != 0) && (qn != 0)) && (tipUzla == 1))
                        {
                            numbersNag.Add(numberNode);

                            //Console.WriteLine($"{numberNode}\t\t{sta}\t\t{pn_est}\t\t{pg_est}");
                        }
                        if ((sta != true) && (pg_est == 0) && ((pg != 0) && (qg != 0)))
                        {
                            numbersGen.Add(numberNode);
                            znachPNagrForGenPTI.Add(znachPNagInUzel);
                            znachQNagrForGenPTI.Add(znachQNagInUzel);

                        }

                    }
                    for (int q = 0; q < numbersNag.Count; q++)
                    {
                        List<double> sumPNach = new List<double>();
                        List<double> sumPKon = new List<double>();
                        List<double> sumQNach = new List<double>();
                        List<double> sumQKon = new List<double>();

                        int countNachTM = 0;
                        int countKonTM = 0;
                        _tableTIChannel.SetSel($"id1={numbersNag[q]}");
                        int n = _tableTIChannel.FindNextSel[-1];
                        while (n != -1)
                        {
                            priv = (string)_privyazka.get_ZN(n);
                            switch (priv)
                            {
                                case "Pнач":
                                    {
                                        znachTMPNach = (double)_znachTm.get_ZN(n);
                                        sumPNach.Add(znachTMPNach);
                                        countNachTM++;
                                        break;
                                    }
                                case "Qнач":
                                    {
                                        znachTMQNach = (double)_znachTm.get_ZN(n);
                                        sumQNach.Add(znachTMQNach);
                                        break;
                                    }
                            }
                            n = _tableTIChannel.FindNextSel[n];
                        }
                        _tableTIChannel.SetSel($"id2={numbersNag[q]}");
                        int kon = _tableTIChannel.FindNextSel[-1];
                        while (kon != -1)
                        {

                            priv = (string)_privyazka.get_ZN(kon);
                            switch (priv)
                            {
                                case "Pкон":
                                    {
                                        znachTMPKon = (double)_znachTm.get_ZN(kon);
                                        sumPKon.Add(znachTMPKon);
                                        countKonTM++;
                                        break;
                                    }
                                case "Qкон":
                                    {
                                        znachTMQKon = (double)_znachTm.get_ZN(kon);
                                        sumQKon.Add(znachTMQKon);
                                        break;
                                    }
                            }
                            kon = _tableTIChannel.FindNextSel[kon];
                        }
                        int countTM = countNachTM + countKonTM;
                        if (!GetCountOfConnections(Rastr, q, numbersNag, countTM))
                        {
                            double ptiPn = (double)(sumPNach.Sum() + sumPKon.Sum());
                            double ptiQn = (double)(sumQNach.Sum() + sumQKon.Sum());
                            _nodes.SetSel($"ny={numbersNag[q]}");
                            int finalvalPn = _nodes.FindNextSel[-1];
                            Pn.set_ZN(finalvalPn, ptiPn);
                            Qn.set_ZN(finalvalPn, ptiQn);
                            //Pn_exist.set_ZN(finalvalPn, 1);

                        }
                        else
                        {
                            Console.WriteLine("Расчет ПТИ невозможен ввиду отсутствия необходимой телеметрии");
                        }


                    }
                    for (int s = 0; s < numbersGen.Count; s++)
                    {
                        List<double> sumPNachForGen = new List<double>();
                        List<double> sumPKonForGen = new List<double>();
                        List<double> sumQNachForGen = new List<double>();
                        List<double> sumQKonForGen = new List<double>();
                        int countNachTMForGen = 0;
                        int countKonTMForGen = 0;
                        _tableTIChannel.SetSel($"id1={numbersGen[s]}");
                        int nGen = _tableTIChannel.FindNextSel[-1];
                        while (nGen != -1)
                        {
                            priv = (string)_privyazka.get_ZN(nGen);
                            switch (priv)
                            {
                                case "Pнач":
                                    {
                                        znachTMPNachForGen = (double)_znachTm.get_ZN(nGen);
                                        sumPNachForGen.Add(znachTMPNachForGen);
                                        countNachTMForGen++;
                                        break;
                                    }
                                case "Qнач":
                                    {
                                        znachTMQNachForGen = (double)_znachTm.get_ZN(nGen);
                                        sumQNachForGen.Add(znachTMQNachForGen);
                                        break;
                                    }
                            }
                            nGen = _tableTIChannel.FindNextSel[nGen];
                        }
                        _tableTIChannel.SetSel($"id2={numbersGen[s]}");
                        int konGen = _tableTIChannel.FindNextSel[-1];
                        while (konGen != -1)
                        {

                            priv = (string)_privyazka.get_ZN(konGen);
                            switch (priv)
                            {
                                case "Pкон":
                                    {
                                        znachTMPKonForGen = (double)_znachTm.get_ZN(konGen);
                                        sumPKonForGen.Add(znachTMPKonForGen);
                                        countKonTMForGen++;
                                        break;
                                    }
                                case "Qкон":
                                    {
                                        znachTMQKonForGen = (double)_znachTm.get_ZN(konGen);
                                        sumQKonForGen.Add(znachTMQKonForGen);
                                        break;
                                    }
                            }
                            konGen = _tableTIChannel.FindNextSel[konGen];
                        }
                        int countTMForGen = countNachTMForGen + countKonTMForGen;
                        if (!GetCountOfConnections(Rastr, s, numbersGen, countTMForGen))
                        {
                            double ptiPg = ((double)(sumPNachForGen.Sum() + sumPKonForGen.Sum()) - znachPNagrForGenPTI[s]) * -1;
                            double ptiQg = ((double)(sumQNachForGen.Sum() + sumQKonForGen.Sum()) - znachQNagrForGenPTI[s]) * -1;
                            _nodes.SetSel($"ny={numbersGen[s]}");
                            int finalvalPg = _nodes.FindNextSel[-1];
                            Pg.set_ZN(finalvalPg, ptiPg);
                            Qg.set_ZN(finalvalPg, ptiQg);
                            //Pg_exist.set_ZN(finalvalPg, 1);
                        }
                        else
                        {
                            Console.WriteLine("Расчет ПТИ невозможен ввиду отсутствия необходимой телеметрии");
                        }
                    }
                    Match match = regex.Match(Convert.ToString(dir));
                    string SaveFile = @"D:\учеба\магистратура\диплом\правильные срезы\тест ПТИ\безнадега\До корр с ПТИ с изменением статуса и фильтрацией";
                    string subpath = string.Concat(match.ToString(), " после корреции");

                    DirectoryInfo dirInfo = new DirectoryInfo(SaveFile);
                    if (!dirInfo.Exists)
                    {
                        dirInfo.Create();
                    }
                    dirInfo.CreateSubdirectory(subpath);

                    // Фильтр
                    COMCKLib.ITI m_TI = new COMCKLib.TI();
                    object SARes = null;
                    int Res = 0;
                    Res = m_TI.FiltrTI_1(Rastr, ref SARes);

                    Rastr.rgm("");
                    Rastr.opf("s");
                    string FullSaveFile = string.Concat(SaveFile, "\\", subpath, "\\", subpath, ".rg2");
                    Rastr.Save(FullSaveFile, "");
                }


            }
        }
        public static bool GetCountOfConnections(IRastr Rastr, int q, List<double> numbers, int countTM)
        {
            // Обращение к таблице ветви
            ITable _vetvi = (ITable)Rastr.Tables.Item("vetv");
            // Обращение к колонке тип ветви
            ICol tipVetvi = (ICol)_vetvi.Cols.Item("tip");

            _vetvi.SetSel($"ip={numbers[q]}");
            int n = _vetvi.FindNextSel[-1];
            int countNachConnections = 0;
            if (n != -1)
            {
                int tipN = (int)tipVetvi.get_ZN(n);
                while (n != -1)
                {
                    if (tipN != 2)
                    {
                        countNachConnections++;

                    }
                    n = _vetvi.FindNextSel[n];
                }
            }
            _vetvi.SetSel($"iq={numbers[q]}");
            int k = _vetvi.FindNextSel[-1];
            int countKonConnections = 0;
            if (k != -1)
            {
                int tipK = (int)tipVetvi.get_ZN(k);
                while (k != -1)
                {
                    if (tipK != 2)
                    {
                        countKonConnections++;
                    }
                    k = _vetvi.FindNextSel[k];
                }
            }
            int sumConnections = countNachConnections + countKonConnections;
            if (sumConnections != countTM)
            {
                return true;
            }
            return false;

        }
    }
}
