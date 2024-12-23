using ASTRALib;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using перенос_бд_на_Web.Models;

namespace перенос_бд_на_Web.Pages.Preprocessing
{
    public class CalculationPTI
    {
        private string dateNow = DateTime.Now.ToString("yyyy_MM_dd");

        public void CalculatePTI( IRastr Rastr, string dirName, ApplicationContext context)
        {

            string PathFile = ($"{dirName}");
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
            // Обращение к колонке название телеметрии
            ICol namePTI = (ICol)_tableTIChannel.Cols.Item("name");
            // Обращение к колонке модель учета
            ICol _model = (ICol)_tableTIChannel.Cols.Item("tip_ti");
            // Обращение к колонке цена 1
            ICol _price1 = (ICol)_tableTIChannel.Cols.Item("price1");
            // Обращение к колонке цена 2
            ICol _price2 = (ICol)_tableTIChannel.Cols.Item("price2");

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
            // Обращение к колонке название узла
            ICol nameUzel = (ICol)_nodes.Cols.Item("name");

            // Обращение к таблице Генераторы УР
            ITable _generatori = (ITable)Rastr.Tables.Item("Generator");
            ICol NumberUzelInGenerators = (ICol)_generatori.Cols.Item("Node");

            // Обращение к таблице реакторы
            ITable _reactors = (ITable)Rastr.Tables.Item("Reactors");
            // Обращение к колонке номер узла в реакторах
            ICol numbeNodeInReactors = (ICol)_reactors.Cols.Item("Id1");
            // Обращение к колонке значения расчетной реактивной мощности
            ICol qRashReactor = (ICol)_reactors.Cols.Item("Qr");
            // Обращение к колонке статус реактора
            ICol statusReactor = (ICol)_reactors.Cols.Item("sta");
            // Обращение к колонке тип реактора (начало/конец)
            ICol tip_Reactor = (ICol)_reactors.Cols.Item("tip");

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
            double znachQInReactors = 0;
            double znachQInReactorsForGen = 0;
            int _nachZnachIdentificator = 9500000;
            int modelTi;
            string nameNode;
            List<double> numbersNag = new List<double>();
            List<double> numbersGen = new List<double>();
            List<double> znachPNagrForGenPTI = new List<double>();
            List<double> znachQNagrForGenPTI = new List<double>();
            List<string> nodeNamesNag = new List<string>();
            List<string> nodeNamesGen = new List<string>();

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
                nameNode = (string)nameUzel.get_ZN(numTM);


                if ((sta != true) && (pn_est == 0) && ((pn != 0) && (qn != 0)))
                {
                    numbersNag.Add(numberNode);
                    nodeNamesNag.Add(nameNode);

                    //Console.WriteLine($"{numberNode}\t\t{sta}\t\t{pn_est}\t\t{pg_est}");
                }
                if ((sta != true) && (pg_est == 0) && ((pg != 0) && (qg != 0)))
                {
                    numbersGen.Add(numberNode);
                    znachPNagrForGenPTI.Add(znachPNagInUzel);
                    znachQNagrForGenPTI.Add(znachQNagInUzel);
                    nodeNamesGen.Add(nameNode);
                }
            }
            for (int q = 0; q < numbersNag.Count; q++)
            {
                List<double> sumPNach = new List<double>();
                List<double> sumPKon = new List<double>();
                List<double> sumQNach = new List<double>();
                List<double> sumQKon = new List<double>();
                List<double> sumQInReactors = new List<double>();
                List<double> nodesNag = new List<double>() { numbersNag[q] };
                znachQInReactors = 0;
                int countNachTM = 0;
                int countKonTM = 0;
                int countTM = 0;
                for (int i = 0; i < 2; i++)
                {
                    GetNodes(ref nodesNag, Rastr, 0);
                    GetNodes(ref nodesNag, Rastr, 1);
                }
                for (int b = 0; b < nodesNag.Count; b++)
                {
                    _tableTIChannel.SetSel($"id1={nodesNag[b]}");
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
                    _tableTIChannel.SetSel($"id2={nodesNag[b]}");
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
                    countTM = countNachTM + countKonTM;
                }
                if (GetCountOfConnections(Rastr, nodesNag, countTM))
                {
                    continue;
                }
                for (int b = 0; b < nodesNag.Count; b++)
                {
                    _reactors.SetSel($"Id1={nodesNag[b]}");
                    int nReactorInNach = _reactors.FindNextSel[-1];
                    if (nReactorInNach != -1)
                    {
                        while (nReactorInNach != -1)
                        {
                            if (((bool)statusReactor.get_ZN(nReactorInNach) == false) &&
                                (((int)tip_Reactor.get_ZN(nReactorInNach) == 1)
                                || ((int)tip_Reactor.get_ZN(nReactorInNach) == 0)))
                            {
                                znachQInReactors = Convert.ToDouble(qRashReactor.get_ZN(nReactorInNach));
                                sumQInReactors.Add(znachQInReactors);
                            }
                            nReactorInNach = _reactors.FindNextSel[nReactorInNach];
                        }
                    }


                    _reactors.SetSel($"Id2={nodesNag[b]}");
                    int nReactorInKon = _reactors.FindNextSel[-1];
                    if (nReactorInKon != -1)
                    {
                        while (nReactorInKon != -1)
                        {
                            if (((bool)statusReactor.get_ZN(nReactorInKon) == false) && ((int)tip_Reactor.get_ZN(nReactorInKon) == 2))
                            {
                                znachQInReactors = (double)qRashReactor.get_ZN(nReactorInKon);
                                sumQInReactors.Add(znachQInReactors);
                            }
                            nReactorInKon = _reactors.FindNextSel[nReactorInKon];
                        }
                    }
                }
                double ptiPn = (double)(sumPNach.Sum() + sumPKon.Sum());
                double ptiQn = (double)(sumQNach.Sum() + sumQKon.Sum() - sumQInReactors.Sum());

                if (ptiPn != 0)
                {
                    GetPTI(Rastr, ptiPn, q, numbersNag, nodeNamesNag, _nachZnachIdentificator, "Pнаг", 4);
                    _nachZnachIdentificator++;
                }
                if (ptiQn != 0)
                {
                    GetPTI(Rastr, ptiQn, q, numbersNag, nodeNamesNag, _nachZnachIdentificator, "Qнаг", 6);
                    _nachZnachIdentificator++;
                }


                _nodes.SetSel($"ny={numbersNag[q]}");
                int finalvalPn = _nodes.FindNextSel[-1];

                Pn_exist.set_ZN(finalvalPn, 1);
                Console.WriteLine($"Проведен расчет ПТИ нагрузки для ТМ № {numbersNag[q]}");
          
            }
            for (int s = 0; s < numbersGen.Count; s++)
            {
                List<double> sumPNachForGen = new List<double>();
                List<double> sumPKonForGen = new List<double>();
                List<double> sumQNachForGen = new List<double>();
                List<double> sumQKonForGen = new List<double>();
                List<double> sumQInReactorsForGen = new List<double>();
                List<double> nodesGen = new List<double>() { numbersGen[s] };
                znachQInReactorsForGen = 0;
                int countNachTMForGen = 0;
                int countKonTMForGen = 0;
                double ptiPg = 0;
                double ptiQg = 0;
                int countTMForGen = 0;
                for (int i = 0; i < 2; i++)
                {
                    GetNodes(ref nodesGen, Rastr, 0);
                    GetNodes(ref nodesGen, Rastr, 1);
                }
                for (int b = 0; b < nodesGen.Count; b++)
                {
                    _tableTIChannel.SetSel($"id1={nodesGen[b]}");
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
                    _tableTIChannel.SetSel($"id2={nodesGen[b]}");
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
                    countTMForGen = countNachTMForGen + countKonTMForGen;
                }
                if (GetCountOfConnections(Rastr, nodesGen, countTMForGen))
                {
                    continue;
                }
                for (int b = 0; b < nodesGen.Count; b++)
                {
                    _reactors.SetSel($"Id1={nodesGen[b]}");
                    int nReactorInNachForGen = _reactors.FindNextSel[-1];
                    if (nReactorInNachForGen != -1)
                    {
                        while (nReactorInNachForGen != -1)
                        {
                            if (((bool)statusReactor.get_ZN(nReactorInNachForGen) == false)
                                && (((int)tip_Reactor.get_ZN(nReactorInNachForGen) == 1)
                                || ((int)tip_Reactor.get_ZN(nReactorInNachForGen) == 0)))
                            {
                                znachQInReactorsForGen = (double)qRashReactor.get_ZN(nReactorInNachForGen);
                                sumQInReactorsForGen.Add(znachQInReactorsForGen);
                            }
                            nReactorInNachForGen = _reactors.FindNextSel[nReactorInNachForGen];
                        }
                    }

                    _reactors.SetSel($"Id2={nodesGen[b]}");
                    int nReactorInKonForGen = _reactors.FindNextSel[-1];
                    if (nReactorInKonForGen != -1)
                    {
                        while (nReactorInKonForGen != -1)
                        {
                            if (((bool)statusReactor.get_ZN(nReactorInKonForGen) == false) && ((int)tip_Reactor.get_ZN(nReactorInKonForGen) == 2))
                            {
                                znachQInReactorsForGen = (double)qRashReactor.get_ZN(nReactorInKonForGen);
                                sumQInReactorsForGen.Add(znachQInReactorsForGen);
                            }
                            nReactorInKonForGen = _reactors.FindNextSel[nReactorInKonForGen];
                        }
                    }
                }

                ptiPg = ((double)(sumPNachForGen.Sum() + sumPKonForGen.Sum()) - znachPNagrForGenPTI[s]) * -1;
                ptiQg = ((double)(sumQNachForGen.Sum() + sumQKonForGen.Sum()) - znachQNagrForGenPTI[s] - sumQInReactorsForGen.Sum()) * -1;

                if (ptiPg != 0)
                {
                    GetPTI(Rastr, ptiPg, s, numbersGen, nodeNamesGen, _nachZnachIdentificator, "Pген", 3);
                    _nachZnachIdentificator++;
                }
                if (ptiQg != 0)
                {
                    GetPTI(Rastr, ptiQg, s, numbersGen, nodeNamesGen, _nachZnachIdentificator, "Qген", 5);
                    _nachZnachIdentificator++;
                }

                _nodes.SetSel($"ny={numbersGen[s]}");
                int finalvalPg = _nodes.FindNextSel[-1];

                Pg_exist.set_ZN(finalvalPg, 1);
                Console.WriteLine($"Для ТМ № {numbersGen[s]} произведен расчет ПТИ для генерации");

            }

            var sliceName = context.slices
                .Where(s => s.SlicePath == dirName)
                .Select(s => s.SliceName)
                .FirstOrDefault();

            string SaveFile = @"C:\Users\User\Desktop\учеба\магистратура\5 семак\диплом по ИТ\тест подготовленные данные";

            // Фильтр
            COMCKLib.ITI m_TI = new COMCKLib.TI();
            object SARes = null;
            int Res = 0;
            Res = m_TI.FiltrTI_1(Rastr, ref SARes);

            Rastr.opf("s");
            string FullSaveFile = Path.Combine(SaveFile, dateNow, sliceName, $"{sliceName}.rg2");
            string directoryPath = Path.GetDirectoryName(FullSaveFile);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            SavePTIData(_tableTIChannel, context, FullSaveFile, sliceName);

            Rastr.Save(FullSaveFile, "");
        }

        public void SavePTIData(ITable _tableTIChannel, ApplicationContext context, string FullSaveFile, string sliceName)
        {
            // Инициализация колонок
            ICol _numberTM = (ICol)_tableTIChannel.Cols.Item("Num");
            ICol _znachTm = (ICol)_tableTIChannel.Cols.Item("ti_val");
            ICol _ocen = (ICol)_tableTIChannel.Cols.Item("ti_ocen");
            ICol _privyazka = (ICol)_tableTIChannel.Cols.Item("prv_num");
            ICol _id1 = (ICol)_tableTIChannel.Cols.Item("id1");
            ICol namePTI = (ICol)_tableTIChannel.Cols.Item("name");
            ICol lagr = (ICol)_tableTIChannel.Cols.Item("lagr");
            ICol type = (ICol)_tableTIChannel.Cols.Item("type");
            ICol cod = (ICol)_tableTIChannel.Cols.Item("cod_oc");

            var sliceID = Guid.NewGuid();

            var slice = new Slices
            {
                SliceID = sliceID,
                SliceName = sliceName,
                SlicePath = FullSaveFile,
                experiment_label = "Подготовленные данные"
            };

            context.slices.Add(slice);

            int rowCount = _tableTIChannel.Size;

            for (int i = 0; i < rowCount; i++)
            {
                var indexTM = Convert.ToDouble(_numberTM.get_ZN(i));
                var izmerValue = Convert.ToDouble(_znachTm.get_ZN(i));
                var ocenValue = Convert.ToDouble(_ocen.get_ZN(i));
                var privyazka = _privyazka.get_ZN(i)?.ToString();
                var id1 = Convert.ToInt32(_id1.get_ZN(i));
                var nameTM = namePTI.get_ZN(i)?.ToString();
                var lagranj = Convert.ToDouble(lagr.get_ZN(i));

                if (IsRelevantTM(type, cod, i))
                {
                    var tmValue = new TMValues
                    {
                        ID = Guid.NewGuid(),
                        IndexTM = indexTM,
                        IzmerValue = izmerValue,
                        OcenValue = ocenValue,
                        Privyazka = privyazka,
                        Id1 = id1,
                        NameTM = nameTM,
                        NumberOfSrez = sliceName,
                        OrderIndex = i, // Порядковый номер строки
                        DeltaOcenIzmer = ocenValue - izmerValue,
                        SliceID = sliceID, // Привязка к текущему Slice
                        Lagranj = lagranj,
                        experiment_label = "Подготовленные данные"
                    };
                    // Добавление объекта в базу данных
                    context.TMValues.Add(tmValue);
                }
                // Сохранение всех данных в базе
            }
            context.SaveChanges();
        }

        static bool IsRelevantTM(ICol typeTM, ICol cod_v_OC, int numTm)
        {
            int typeValue = (int)typeTM.get_ZN(numTm);
            int codValue = (int)cod_v_OC.get_ZN(numTm);

            return (typeValue == 0 || typeValue == 2) && codValue == 1;
        }

        public static void GetNodes(ref List<double> arrNachVetv, IRastr Rastr, int status)
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
                    int n = _table_vetvi.FindNextSel[-1];
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
                    int n = _table_vetvi.FindNextSel[-1];
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
        public static void GetPTI(IRastr Rastr, double ptiPn, int q, List<double> numbersNag, List<string> nodeNamesNag, int _nachZnachIdentificator, string priv, int privint)
        {
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
            // Обращение к колонке название телеметрии
            ICol namePTI = (ICol)_tableTIChannel.Cols.Item("name");
            // Обращение к колонке модель учета
            ICol _model = (ICol)_tableTIChannel.Cols.Item("tip_ti");
            // Обращение к колонке цена 1
            ICol _price1 = (ICol)_tableTIChannel.Cols.Item("price1");
            // Обращение к колонке цена 2
            ICol _price2 = (ICol)_tableTIChannel.Cols.Item("price2");

            _tableTIChannel.AddRow();
            int numbersting = _tableTIChannel.Size - 1;

            _numberTM.set_ZN(numbersting, _nachZnachIdentificator);
            tip.set_ZN(numbersting, 0);
            namePTI.set_ZN(numbersting, string.Concat("ПТИ", " ", nodeNamesNag[q], " ", priv));
            _znachTm.set_ZN(numbersting, ptiPn);
            _privyazka.set_ZN(numbersting, privint);
            _id1.set_ZN(numbersting, numbersNag[q]);
            _cod.set_ZN(numbersting, 1);
            _model.set_ZN(numbersting, 1);
            _price1.set_ZN(numbersting, 20);
            _price2.set_ZN(numbersting, 20);
        }
        public static bool GetCountOfConnections(IRastr Rastr, List<double> numbers, int countTM)
        {
            // Обращение к таблице ветви
            ITable _vetvi = (ITable)Rastr.Tables.Item("vetv");
            // Обращение к колонке тип ветви
            ICol tipVetvi = (ICol)_vetvi.Cols.Item("tip");
            int countKonConnections = 0;
            int countNachConnections = 0;
            for (int q = 0; q < numbers.Count; q++)
            {
                _vetvi.SetSel($"ip={numbers[q]}");
                int n = _vetvi.FindNextSel[-1];

                if (n != -1)
                {

                    while (n != -1)
                    {
                        int tipN = (int)tipVetvi.get_ZN(n);
                        if (tipN != 2)
                        {
                            countNachConnections++;

                        }
                        n = _vetvi.FindNextSel[n];
                    }
                }
                _vetvi.SetSel($"iq={numbers[q]}");
                int k = _vetvi.FindNextSel[-1];

                if (k != -1)
                {

                    while (k != -1)
                    {
                        int tipK = (int)tipVetvi.get_ZN(k);
                        if (tipK != 2)
                        {
                            countKonConnections++;
                        }
                        k = _vetvi.FindNextSel[k];
                    }
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