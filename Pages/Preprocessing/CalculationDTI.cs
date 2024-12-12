using ASTRALib;
using System.Text.RegularExpressions;
using перенос_бд_на_Web.Models;

namespace перенос_бд_на_Web.Pages.Preprocessing
{
    public class CalculationDTI
    {
        public void CalculateDTIAndPTI (string dirName, ApplicationContext context)
        {
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
            // Обращение к колонке статуса узла
            ICol statusNode = (ICol)_nodes.Cols.Item("sta");
            // Обращение к колонке Pg_есть
            ICol Pg_exist = (ICol)_nodes.Cols.Item("exist_gen");
            // Обращение к колонке Pn_есть
            ICol Pn_exist = (ICol)_nodes.Cols.Item("exist_load");
            // Обращение к колонке Pн
            ICol Pn = (ICol)_nodes.Cols.Item("pn");
            // Обращение к колонке Qн
            ICol Qn = (ICol)_nodes.Cols.Item("qn");
            // Обращение к колонке Pг
            ICol Pg = (ICol)_nodes.Cols.Item("pg");
            // Обращение к колонке Pг
            ICol Qg = (ICol)_nodes.Cols.Item("qg");
            // Обращение к колонке тип узла
            ICol _tipUzla = (ICol)_nodes.Cols.Item("tip");
            // Обращение к колонке название узла
            ICol nameUzel = (ICol)_nodes.Cols.Item("name");
            // Обращение к колонке U_ном
            ICol u_nom = (ICol)_nodes.Cols.Item("uhom");
     
     
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
     
            // Обращение к таблице ветви
            ITable _vetvi = (ITable)Rastr.Tables.Item("vetv");
            // Обращение к колонке Nнач
            ICol n_nach = (ICol)_vetvi.Cols.Item("ip");
            // Обращение к колонке Nкон
            ICol n_kon = (ICol)_vetvi.Cols.Item("iq");
            // Обращение к таблице P_нач_ти
            ICol _ti_p_in_nach = (ICol)_vetvi.Cols.Item("ti_pl_ip");
            // Обращение к таблице P_кон_ти
            ICol _ti_p_in_kon = (ICol)_vetvi.Cols.Item("ti_pl_iq");
            // Обращение к таблице Q_нач_ти
            ICol _ti_q_in_nach = (ICol)_vetvi.Cols.Item("ti_ql_ip");
            // Обращение к таблице P_кон_ти
            ICol _ti_q_in_kon = (ICol)_vetvi.Cols.Item("ti_ql_iq");
            // Обращение к колонке tip
            ICol tipVetvi = (ICol)_vetvi.Cols.Item("tip");
            // Обращение к колонке R линии
            ICol r_lin = (ICol)_vetvi.Cols.Item("r");
            // Обращение к колонке X линии 
            ICol x_lin = (ICol)_vetvi.Cols.Item("x");
            // Обращение к колонке B линии
            ICol b_lin = (ICol)_vetvi.Cols.Item("b");
     
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
            int _nachZnachIdentificator = 9000000;
            int modelTi;
            string nameNode;
            int nPar;
     
            List<string> nodeNamesNag = new List<string>();
            List<string> nodeNamesGen = new List<string>();
            List<double> numbersNag = new List<double>();
            List<double> numbersGen = new List<double>();
     
            for (int numUz = 0; numUz < schetUzli; numUz++)
            {
                sta = (bool)statusNode.get_ZN(numUz);
                numberNode = (int)_numberNode.get_ZN(numUz);
                pg_est = (int)Pg_exist.get_ZN(numUz);
                pn_est = (int)Pn_exist.get_ZN(numUz);
                pn = (double)Pn.get_ZN(numUz);
                qn = (double)Qn.get_ZN(numUz);
                pg = (double)Pg.get_ZN(numUz);
                qg = (double)Qg.get_ZN(numUz);
                tipUzla = (int)_tipUzla.get_ZN(numUz);
                nameNode = (string)nameUzel.get_ZN(numUz);
     
     
                if ((sta != true) && (pn_est == 0) && ((pn != 0) && (qn != 0)) && (tipUzla == 1))
                {
                    numbersNag.Add(numberNode);
                    nodeNamesNag.Add(nameNode);
                }
                if ((sta != true) && (pg_est == 0) && (pg != 0) && (qg != 0))
                {
                    numbersGen.Add(numberNode);
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
                for (int i = 0; i < 2; i++)
                {
                    GetNodes(ref nodesNag, Rastr, 0);
                    GetNodes(ref nodesNag, Rastr, 1);
                }
     
                for (int b = 0; b < nodesNag.Count; b++)
                {
     
                    if (FiltrTM(Rastr, nodesNag[b]))
                    {
                        break;
                    }
                    CalculateDTI(Rastr, nodesNag[b], nodeNamesNag[q], ref _nachZnachIdentificator);
     
                }
                Console.WriteLine($"Проведен расчет ПТИ нагрузки для ТМ № {numbersNag[q]}");
            }
            for (int s = 0; s < numbersGen.Count; s++)
            {
                List<double> sumPNachGen = new List<double>();
                List<double> sumPKonGen = new List<double>();
                List<double> sumQNachGen = new List<double>();
                List<double> sumQKonGen = new List<double>();
                List<double> sumQInReactorsGen = new List<double>();
                List<double> nodesGen = new List<double>() { numbersGen[s] };
     
                znachQInReactors = 0;
                for (int i = 0; i < 2; i++)
                {
                    GetNodes(ref nodesGen, Rastr, 0);
                    GetNodes(ref nodesGen, Rastr, 1);
                }
     
                for (int b = 0; b < nodesGen.Count; b++)
                {
     
                    if (FiltrTM(Rastr, nodesGen[b]))
                    {
                        break;
                    }
                    CalculateDTI(Rastr, nodesGen[b], nodeNamesGen[s], ref _nachZnachIdentificator);
     
     
                }
                Console.WriteLine($"Проведен расчет ДТИ перетока для ТМ № {numbersGen[s]}");
            }
     
            CalculationPTI ptiCalculation = new CalculationPTI();
            ptiCalculation.CalculatePTI(Rastr, PathFile, context);
        }
            
        
        public static void CalculateDTI(IRastr Rastr, double nodesNag, string nodeNamesNag, ref int _nachZnachIdentificator)
        {
            // Обращение к таблице ветви
            ITable _vetvi = (ITable)Rastr.Tables.Item("vetv");
            // Обращение к колонке Nнач
            ICol n_nach = (ICol)_vetvi.Cols.Item("ip");
            // Обращение к колонке Nкон
            ICol n_kon = (ICol)_vetvi.Cols.Item("iq");
            // Обращение к таблице P_нач_ти
            ICol _ti_p_in_nach = (ICol)_vetvi.Cols.Item("ti_pl_ip");
            // Обращение к таблице P_кон_ти
            ICol _ti_p_in_kon = (ICol)_vetvi.Cols.Item("ti_pl_iq");
            // Обращение к таблице Q_нач_ти
            ICol _ti_q_in_nach = (ICol)_vetvi.Cols.Item("ti_ql_ip");
            // Обращение к таблице P_кон_ти
            ICol _ti_q_in_kon = (ICol)_vetvi.Cols.Item("ti_ql_iq");
            // Обращение к колонке tip
            ICol tipVetvi = (ICol)_vetvi.Cols.Item("tip");
            // Обращение к колонке R линии
            ICol r_lin = (ICol)_vetvi.Cols.Item("r");
            // Обращение к колонке X линии 
            ICol x_lin = (ICol)_vetvi.Cols.Item("x");
            // Обращение к колонке B линии
            ICol b_lin = (ICol)_vetvi.Cols.Item("b");
            // Обращение к колонке номер параллельности
            ICol nPar = (ICol)_vetvi.Cols.Item("np");
            // Обращение к колонке наименование ветви
            ICol nameOfVetv = (ICol)_vetvi.Cols.Item("name");



            // Обращение к таблице узлы
            ITable _nodes = (ITable)Rastr.Tables.Item("node");
            // Обращение к колонке номер узла
            ICol _numberNode = (ICol)_nodes.Cols.Item("ny");
            // Обращение к колонке статуса узла
            ICol statusNode = (ICol)_nodes.Cols.Item("sta");
            // Обращение к колонке Pg_есть
            ICol Pg_exist = (ICol)_nodes.Cols.Item("exist_gen");
            // Обращение к колонке Pn_есть
            ICol Pn_exist = (ICol)_nodes.Cols.Item("exist_load");
            // Обращение к колонке Pн
            ICol Pn = (ICol)_nodes.Cols.Item("pn");
            // Обращение к колонке Qн
            ICol Qn = (ICol)_nodes.Cols.Item("qn");
            // Обращение к колонке Pг
            ICol Pg = (ICol)_nodes.Cols.Item("pg");
            // Обращение к колонке Pг
            ICol Qg = (ICol)_nodes.Cols.Item("qg");
            // Обращение к колонке тип узла
            ICol _tipUzla = (ICol)_nodes.Cols.Item("tip");
            // Обращение к колонке название узла
            ICol nameUzel = (ICol)_nodes.Cols.Item("name");
            // Обращение к колонке U_ном
            ICol u_nom = (ICol)_nodes.Cols.Item("uhom");


            double pNach;
            double pKon;
            double qNach;
            double qKon;
            double nominalVoltage;

            _vetvi.SetSel($"ip={nodesNag}");
            int n = _vetvi.FindNextSel[-1];
            while (n != -1)
            {
                int tipN = (int)tipVetvi.get_ZN(n);
                if (tipN != 2)
                {
                    pNach = (double)_ti_p_in_nach.get_ZN(n);
                    pKon = (double)_ti_p_in_kon.get_ZN(n);
                    qNach = (double)_ti_q_in_nach.get_ZN(n);
                    qKon = (double)_ti_q_in_kon.get_ZN(n);
                    double B;
                    double R;
                    double X;
                    if ((pNach == 0) && (qNach == 0))
                    {
                        int konUz = (int)n_kon.get_ZN(n);
                        int nachUz = (int)n_nach.get_ZN(n);
                        int nparallel = (int)nPar.get_ZN(n);
                        string nameVetv = (string)nameOfVetv.get_ZN(n);
                        _nodes.SetSel($"ny={nodesNag}");
                        int findNode = _nodes.FindNextSel[-1];
                        nominalVoltage = (double)u_nom.get_ZN(findNode);
                        B = (double)b_lin.get_ZN(n);
                        double Qc = 0.5 * Math.Pow(nominalVoltage, 2) * B * Math.Pow(10, -6);
                        if (qKon < 0)
                        {
                            qNach = qKon + Qc;
                        }
                        else
                        {
                            qNach = qKon - Qc;
                        }
                        R = (double)r_lin.get_ZN(n);
                        X = (double)x_lin.get_ZN(n);
                        double dP = (Math.Pow(pKon, 2) + Math.Pow(qNach, 2)) / Math.Pow(nominalVoltage, 2) * R;
                        double dQ = (Math.Pow(pKon, 2) + Math.Pow(qNach, 2)) / Math.Pow(nominalVoltage, 2) * X;
                        if (qNach < 0)
                        {
                            qNach = qNach - dQ + Qc;
                            qNach = qNach * (-1);
                        }
                        else
                        {
                            qNach = qNach + dQ - Qc;
                            qNach *= (-1);
                        }
                        if (pKon < 0)
                        {
                            pNach = (pKon) - dP;
                            pNach *= (-1);
                        }
                        else
                        {
                            pNach = (pKon) + dP;
                            pNach *= (-1);
                        }

                        if (qNach != 0)
                        {
                            AddPTIForVetv(Rastr, qNach, nachUz, nameVetv, _nachZnachIdentificator, "Qнач", 11, konUz, nparallel);
                            Console.WriteLine("Создано ДТИ");
                            _nachZnachIdentificator++;
                        }
                        if (pNach != 0)
                        {
                            AddPTIForVetv(Rastr, pNach, nachUz, nameVetv, _nachZnachIdentificator, "Pнач", 9, konUz, nparallel);
                            Console.WriteLine("Создано ДТИ");
                            _nachZnachIdentificator++;
                        }
                    }

                }
                n = _vetvi.FindNextSel[n];
            }
            _vetvi.SetSel($"iq={nodesNag}");
            n = _vetvi.FindNextSel[-1];
            while (n != -1)
            {
                int tipN = (int)tipVetvi.get_ZN(n);
                if (tipN != 2)
                {
                    pNach = (double)_ti_p_in_nach.get_ZN(n);
                    pKon = (double)_ti_p_in_kon.get_ZN(n);
                    qNach = (double)_ti_q_in_nach.get_ZN(n);
                    qKon = (double)_ti_q_in_kon.get_ZN(n);
                    double B;
                    double R;
                    double X;
                    if ((pKon == 0) && (qKon == 0))
                    {
                        int konUz = (int)n_kon.get_ZN(n);
                        int nachUz = (int)n_nach.get_ZN(n);
                        int nparallel = (int)nPar.get_ZN(n);
                        string nameVetv = (string)nameOfVetv.get_ZN(n);
                        _nodes.SetSel($"ny={nodesNag}");
                        int findNode = _nodes.FindNextSel[-1];
                        nominalVoltage = (double)u_nom.get_ZN(findNode);
                        B = (double)b_lin.get_ZN(n);
                        double Qc = 0.5 * Math.Pow(nominalVoltage, 2) * B * Math.Pow(10, -6);
                        if (qNach < 0)
                        {
                            qKon = qNach - Qc;
                        }
                        else
                        {
                            qKon = qNach + Qc;
                        }
                        R = (double)r_lin.get_ZN(n);
                        X = (double)x_lin.get_ZN(n);
                        double dP = (Math.Pow(pNach, 2) + Math.Pow(qKon, 2)) / Math.Pow(nominalVoltage, 2) * R;
                        double dQ = (Math.Pow(pNach, 2) + Math.Pow(qKon, 2)) / Math.Pow(nominalVoltage, 2) * X;
                        if (qKon < 0)
                        {
                            qKon = (qKon) + dQ - Qc;
                            qKon *= (-1);
                        }
                        else
                        {
                            qKon = (qKon) - dQ + Qc;
                            qKon *= (-1);
                        }
                        if (pNach < 0)
                        {
                            pKon = (pNach) + dP;
                            pKon *= (-1);
                        }
                        else
                        {
                            pKon = (pNach) - dP;
                            pKon *= (-1);
                        }
                        if (qKon != 0)
                        {
                            AddPTIForVetv(Rastr, qKon, nachUz, nameVetv, _nachZnachIdentificator, "Qкон", 12, konUz, nparallel);
                            Console.WriteLine("Создано ДТИ");
                            _nachZnachIdentificator++;
                        }
                        if (pKon != 0)
                        {
                            AddPTIForVetv(Rastr, pKon, nachUz, nameVetv, _nachZnachIdentificator, "Pкон", 10, konUz, nparallel);
                            Console.WriteLine("Создано ДТИ");
                            _nachZnachIdentificator++;
                        }
                    }
                }
                n = _vetvi.FindNextSel[n];
            }
        }
        public static int GetParralel(IRastr Rastr, double numbersNag, int konUz)
        {
            // Обращение к таблице ТИ:каналы
            ITable _tableTIChannel = (ITable)Rastr.Tables.Item("ti");


            // Обращение к таблице ветви
            ITable _vetv = (ITable)Rastr.Tables.Item("vetv");
            // Обращение к колонке номер параллельности
            ICol nPar = (ICol)_vetv.Cols.Item("np");
            // Обращение к колонке наименование ветви
            ICol nameOfVetv = (ICol)_vetv.Cols.Item("name");
            // Обращение к колонке начало ветви
            ICol nach_vetv = (ICol)_vetv.Cols.Item("ip");
            // Обращение к колонке конец ветви 
            ICol kon_vetv = (ICol)_vetv.Cols.Item("iq");

            int nParallel;
            string nameVetv;
            int nach;
            int kon;


            _vetv.SetSel($"ip={numbersNag} iq ={konUz}");
            int n = _vetv.FindNextSel[-1];
            while (n != -1)
            {
                kon = (int)kon_vetv.get_ZN(n);
                nParallel = (int)nPar.get_ZN(n);
                nameVetv = (string)nameOfVetv.get_ZN(n);
                return nParallel;

                n = _vetv.FindNextSel[n];
            }
            return -1;
        }
        public static bool FiltrTM(IRastr Rastr, double nodesNag)
        {
            // Обращение к таблице ветви
            ITable _vetvi = (ITable)Rastr.Tables.Item("vetv");
            // Обращение к колонке Nнач

            // Обращение к таблице P_нач_ти
            ICol _ti_p_in_nach = (ICol)_vetvi.Cols.Item("ti_pl_ip");
            // Обращение к таблице P_кон_ти
            ICol _ti_p_in_kon = (ICol)_vetvi.Cols.Item("ti_pl_iq");
            // Обращение к таблице Q_нач_ти
            ICol _ti_q_in_nach = (ICol)_vetvi.Cols.Item("ti_ql_ip");
            // Обращение к таблице P_кон_ти
            ICol _ti_q_in_kon = (ICol)_vetvi.Cols.Item("ti_ql_iq");
            // Обращение к колонке tip
            ICol tipVetvi = (ICol)_vetvi.Cols.Item("tip");


            double pNach;
            double pKon;
            double qNach;
            double qKon;
            _vetvi.SetSel($"ip={nodesNag}");
            int n = _vetvi.FindNextSel[-1];

            while (n != -1)
            {
                int tipN = (int)tipVetvi.get_ZN(n);
                if (tipN != 2)
                {
                    pNach = (double)_ti_p_in_nach.get_ZN(n);
                    pKon = (double)_ti_p_in_kon.get_ZN(n);
                    qNach = (double)_ti_q_in_nach.get_ZN(n);
                    qKon = (double)_ti_q_in_kon.get_ZN(n);
                    if (!((pNach != 0 && qNach != 0) || (pKon != 0 && qKon != 0)))
                    {
                        return true;
                    }
                }
                n = _vetvi.FindNextSel[n];
            }

            _vetvi.SetSel($"iq={nodesNag}");
            n = _vetvi.FindNextSel[-1];
            while (n != -1)
            {
                int tipN = (int)tipVetvi.get_ZN(n);
                if (tipN != 2)
                {
                    pNach = (double)_ti_p_in_nach.get_ZN(n);
                    pKon = (double)_ti_p_in_kon.get_ZN(n);
                    qNach = (double)_ti_q_in_nach.get_ZN(n);
                    qKon = (double)_ti_q_in_kon.get_ZN(n);
                    if (!((pNach != 0 && qNach != 0) || (pKon != 0 && qKon != 0)))
                    {
                        return true;
                    }

                }
                n = _vetvi.FindNextSel[n];
            }
            return false;
        }
        public static void GetTM(IRastr Rastr, double id, int nachOrKon, ref List<double> sumPNach, ref List<double> sumQNach)
        {
            // Обращение к таблице ТИ:каналы
            ITable _tableTIChannel = (ITable)Rastr.Tables.Item("ti");
            // Обращение к колонке значений
            ICol _znachTm = (ICol)_tableTIChannel.Cols.Item("ti_val");
            // Обращение к колонке привязка
            ICol _privyazka = (ICol)_tableTIChannel.Cols.Item("prv_num");
            string priv;
            double znachTMPNach;
            double znachTMQNach;
            Dictionary<int, string[]> priviazka = new Dictionary<int, string[]>()
            {
                { 1, new[] { "Pнач", "Qнач" }},
                { 2, new[] { "Pкон", "Qкон" }}
            };
            _tableTIChannel.SetSel($"id{nachOrKon}={id}");
            int n = _tableTIChannel.FindNextSel[-1];
            string P = priviazka[nachOrKon][0];
            string Q = priviazka[nachOrKon][1];
            while (n != -1)
            {
                priv = (string)_privyazka.get_ZN(n);
                if (priv == P)
                {
                    znachTMPNach = (double)_znachTm.get_ZN(n);
                    sumPNach.Add(znachTMPNach);
                }
                else if (priv == Q)
                {
                    znachTMQNach = (double)_znachTm.get_ZN(n);
                    sumQNach.Add(znachTMQNach);
                }

                n = _tableTIChannel.FindNextSel[n];
            }

        }

        public static void AddPTIForVetv(IRastr Rastr, double ptiPn, double numbersNag, string nodeNamesNag, int _nachZnachIdentificator, string priv, int privint, int konUz, int parallelnost)
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
            // Обращение к колонке id3
            ICol _id3 = (ICol)_tableTIChannel.Cols.Item("id3");
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

            // Обращение к таблице ветви
            ITable _vetv = (ITable)Rastr.Tables.Item("vetv");
            // Обращение к колонке номер параллельности
            ICol nPar = (ICol)_vetv.Cols.Item("np");
            // Обращение к колонке наименование ветви
            ICol nameOfVetv = (ICol)_vetv.Cols.Item("name");
            // Обращение к колонке начало ветви
            ICol nach_vetv = (ICol)_vetv.Cols.Item("ip");
            // Обращение к колонке конец ветви 
            ICol kon_vetv = (ICol)_vetv.Cols.Item("iq");


            _tableTIChannel.AddRow();
            int numbersting = _tableTIChannel.Size - 1;
            _numberTM.set_ZN(numbersting, _nachZnachIdentificator);
            tip.set_ZN(numbersting, 0);
            namePTI.set_ZN(numbersting, string.Concat("ПТИ", " ", nodeNamesNag, " ", priv));
            _znachTm.set_ZN(numbersting, ptiPn);
            _privyazka.set_ZN(numbersting, privint);
            _id1.set_ZN(numbersting, numbersNag);
            _cod.set_ZN(numbersting, 1);
            _model.set_ZN(numbersting, 1);
            _price1.set_ZN(numbersting, 20);
            _price2.set_ZN(numbersting, 20);
            _id2.set_ZN(numbersting, konUz);
            _id3.set_ZN(numbersting, parallelnost);

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
    }
}
