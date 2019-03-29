using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Współbiegi
{
    public class DrogaJednokierunkowaZPromem
    {
        private int aktywniKierowcy;    //kierowcy oczekujący na wjazd na prom
        private int dzialajacyKierowcy; //kierowcy którzy wjechali na prom
        //private int ilukierowcow;
        //private int buffor;
        private Semaphore wjazd;        //semafor do wstrzymywania czytelników
        private Semaphore zjazd;       //semafor do wstrzymywania pisarzy
        private Semaphore pusty;        //semafory pusty oraz pelny slużą do sygnalizacji
        private Semaphore pelny;        //liczby elementów w buforze
        private Semaphore dostep;       //semafor chroniący dostęp do sekcji krytycznej

        private ArrayList bufor;        //ilość miejsc na promie
        private Thread[] kierowcy;      //tablica wątków kierowcow
        private Thread prom;            //wątek promu
        private Boolean lewo;           //flagi lewo i prawo okreslaja przy ktorym
        private Boolean prawo;          //brzegu znajduje sie prom
        //private Boolean plyne;          //faga okreslajaca ruch promu

        public DrogaJednokierunkowaZPromem(int iluKierowcow, int jakiBufor)
        {
            pusty = new Semaphore(jakiBufor, jakiBufor);
            pelny = new Semaphore(0, jakiBufor);
            wjazd = new Semaphore(1, 1);
            zjazd = new Semaphore(0, 1);
            dostep = new Semaphore(1, 1);    //Semafor dostep ma być semaforem binarnym.
            bufor = new ArrayList();
            kierowcy = new Thread[iluKierowcow];
            //ilukierowcow = iluKierowcow;
            //buffor = jakiBufor;
            lewo = true;
            prawo = false;
            for (int i = 0; i < iluKierowcow; i++)
            {
                kierowcy[i] = new Thread(new ThreadStart(this.Kierowca));
                kierowcy[i].Name = "Kierowca " + i;
            }

            prom = new Thread(new ThreadStart(this.Prom));
            prom.Name = "Prom";
        }

        
        public void Startuj()
        {
            foreach (Thread t in kierowcy)
            {
                t.Start();
            }

            prom.Start();
        }
        public void Zakoncz()
        {
                foreach (Thread t in kierowcy)
                {
                    t.Interrupt();
                }
                
                prom.Interrupt();
                Console.WriteLine("Koniec kursów na dzisiaj");
        }

        
        public int Kierowcy()
        {
            return dzialajacyKierowcy;
        }
        public int Getdzialajacy()
        {
            return dzialajacyKierowcy;
        }

        public void Kierowca()
        {
            try
            {
                Random rand = new Random();
                while (true)
                {
                    Thread.Sleep(rand.Next(1000, 7500));
                    int j = 0;
                    int i = 0;
                    aktywniKierowcy++;
                    Console.WriteLine("{0} nadjeżdża", Thread.CurrentThread.Name);
                    Thread.Sleep(rand.Next(2000, 3500));
                    pusty.WaitOne();    //sprawdzamy czy bufor nie jest pełny
                    wjazd.WaitOne();    //sprawdzamy czy prom jest na lewym brzegu
                    dostep.WaitOne();   //wchodzimy do sekcji krytycznej
                    
                    if (lewo == true)
                    {
                        dzialajacyKierowcy++;
                        bufor.Add(j);
                        Thread.Sleep(1000);
                        Console.WriteLine("{0} zajął miejsce", Thread.CurrentThread.Name);
                        j++;
                    }
                   
                    dostep.Release();
                    wjazd.Release();
                    if (lewo == false && prawo == false)
                    {
                        Console.WriteLine("{0} czeka", Thread.CurrentThread.Name);
                        Thread.Sleep(5000);
                    }
                    zjazd.WaitOne();
                    dostep.WaitOne();
                    
                    if (prawo == true)
                    {
                        i = (int)bufor[0];
                        bufor.RemoveAt(0);
                        Thread.Sleep(1000);
                        Console.WriteLine("{0} zwolnił miejsce", Thread.CurrentThread.Name);
                        dzialajacyKierowcy--;
                        
                        pusty.Release();
                    }
                    
                    dostep.Release();
                    zjazd.Release();
                    //Thread.Sleep(rand.Next(800, 2200));
                    aktywniKierowcy--;
                    Thread.Sleep(rand.Next(2000, 3500));
                    Console.WriteLine("{0} odjechał", Thread.CurrentThread.Name);
                }
            }
            catch (ThreadInterruptedException)
            { }
        }
        public void Prom()
        {
            try
            {
                Random rand = new Random();
                while (true)
                {
                    //Thread.Sleep(5000);
                    /* dostep.WaitOne();
                     if (lewo == true && dzialajacyKierowcy == 0)
                     {
                         Thread.Sleep(500);
                         Console.WriteLine("{0} czeka na samochody ", Thread.CurrentThread.Name);
                     }
                     dostep.Release();*/
                    dostep.WaitOne();
                    if(lewo == true)
                    {
                        if (dzialajacyKierowcy == 0)
                        {
                            
                            Console.WriteLine();
                            Console.WriteLine("{0} czeka na samochód ", Thread.CurrentThread.Name);
                            Console.WriteLine();
                            Thread.Sleep(2000);
                        }
                        else
                        //if (dzialajacyKierowcy != 0)
                        {
                            wjazd.WaitOne();
                            lewo = false;
                            Console.WriteLine();
                            Console.WriteLine("{0} odpływa od lewego brzegu", Thread.CurrentThread.Name);
                            Thread.Sleep(5000);
                            Console.WriteLine("{0} dopłynął do prawego brzegu", Thread.CurrentThread.Name);
                            Console.WriteLine();
                            prawo = true;
                            zjazd.Release();
                        }
                    }
                    dostep.Release();
                    dostep.WaitOne();
                    if(prawo == true )
                    {
                        if (dzialajacyKierowcy == 0)
                        {
                            zjazd.WaitOne();
                            prawo = false;
                            Console.WriteLine();
                            Console.WriteLine("{0} odpływa od prawego brzegu", Thread.CurrentThread.Name);
                            Thread.Sleep(5000);
                            Console.WriteLine("{0} dopłynął do lewego brzegu", Thread.CurrentThread.Name);
                            Console.WriteLine();
                            lewo = true;
                            wjazd.Release();
                        }
                        if (dzialajacyKierowcy != 0)
                        {
                            Thread.Sleep(20);
                            Console.WriteLine("{0} czeka na zjazd samochodów ", Thread.CurrentThread.Name);
                            Console.WriteLine();
                        }
                    }
                    dostep.Release();
                    Thread.Sleep(5000);
                }
            }
            catch (ThreadInterruptedException)
            {
            }
        }

        static void Main(string[] args)
        {
           // int m = 1;
           Console.WriteLine("------------------------------------");
           Console.WriteLine("*  Droga jednokierunkowa z promem  *");
           Console.WriteLine("------------------------------------");
            DrogaJednokierunkowaZPromem dp = new DrogaJednokierunkowaZPromem(4, 2);
            
            dp.Startuj();

            Thread.Sleep(100000);  //Czekamy przez wybrany czas
            while (dp.Getdzialajacy() != 0)
                {
                    //Console.WriteLine("Ostatni kurs");
                    Thread.Sleep(500);
                }
            dp.Zakoncz();
            Console.Read();
        }
    }
}

//pazmarcin@gmail.com