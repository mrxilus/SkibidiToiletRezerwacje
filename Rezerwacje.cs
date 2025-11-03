using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Rezerwacje
{
    public class Wydarzenie
    {
        public string Nazwa { get; private set; }
        public DateTime Data { get; private set; }
        public int LiczbaMiejsc { get; private set; }
        public int LiczbaZarezerwowanych { get; private set; }

        public Wydarzenie(string nazwa, DateTime data, int liczbaMiejsc, int liczbaZarezerwowanych = 0)
        {
            if (string.IsNullOrWhiteSpace(nazwa))
                throw new ArgumentException("Nazwa wydarzenia nie może być pusta.");

            if (liczbaMiejsc <= 0)
                throw new ArgumentException("Liczba miejsc musi być większa niż zero.");

            if (liczbaZarezerwowanych < 0 || liczbaZarezerwowanych > liczbaMiejsc)
                throw new ArgumentException("Nieprawidłowa liczba zarezerwowanych miejsc.");

            Nazwa = nazwa.Trim();
            Data = data;
            LiczbaMiejsc = liczbaMiejsc;
            LiczbaZarezerwowanych = liczbaZarezerwowanych;
        }

        public (bool Sukces, string Komunikat) Zarezerwuj(int ile)
        {
            if (ile <= 0)
                return (false, "Liczba rezerwowanych miejsc musi być dodatnia.");

            int wolne = LiczbaWolnychMiejsc();
            if (ile > wolne)
                return (false, "Brak wystarczającej liczby miejsc.");

            LiczbaZarezerwowanych += ile;
            return (true, $"Zarezerwowano {ile} miejsc. Wolnych: {LiczbaWolnychMiejsc()}.");
        }

        public (bool Sukces, string Komunikat) Anuluj(int ile)
        {
            if (ile <= 0)
                return (false, "Liczba anulowanych miejsc musi być dodatnia.");

            if (ile > LiczbaZarezerwowanych)
                return (false, "Nie można anulować więcej miejsc niż zarezerwowano.");

            LiczbaZarezerwowanych -= ile;
            return (true, $"Anulowano {ile} miejsc. Zarezerwowane: {LiczbaZarezerwowanych}.");
        }

        public int LiczbaWolnychMiejsc() => LiczbaMiejsc - LiczbaZarezerwowanych;

        public string OpisTekstowy()
        {
            return $"{Nazwa} | {Data:yyyy-MM-dd HH:mm} | Miejsca: {LiczbaMiejsc} | Zarezerwowane: {LiczbaZarezerwowanych} | Wolne: {LiczbaWolnychMiejsc()}";
        }

        public string ToCsv()
        {
            return $"{Escape(Nazwa)};{Data:yyyy-MM-dd HH:mm};{LiczbaMiejsc};{LiczbaZarezerwowanych}";
        }

        public static Wydarzenie FromCsv(string line)
        {
            var parts = SplitCsv(line, ';', 4);
            if (parts == null)
                throw new FormatException("Nieprawidłowy format wiersza danych.");

            string nazwa = Unescape(parts[0]);
            if (!DateTime.TryParseExact(parts[1], "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
                throw new FormatException("Nieprawidłowy format daty.");

            if (!int.TryParse(parts[2], out int liczbaMiejsc))
                throw new FormatException("Nieprawidłowa liczba miejsc.");

            if (!int.TryParse(parts[3], out int liczbaZarezerwowanych))
                throw new FormatException("Nieprawidłowa liczba zarezerwowanych miejsc.");

            return new Wydarzenie(nazwa, data, liczbaMiejsc, liczbaZarezerwowanych);
        }

        private static string Escape(string s)
        {
            if (s.Contains(';') || s.Contains('"'))
            {
                return $"\"{s.Replace("\"", "\"\"")}\"";
            }
            return s;
        }

        private static string Unescape(string s)
        {
            s = s.Trim();
            if (s.StartsWith("\"") && s.EndsWith("\""))
            {
                s = s.Substring(1, s.Length - 2).Replace("\"\"", "\"");
            }
            return s;
        }

        private static string[] SplitCsv(string input, char separator, int expectedParts)
        {
            var parts = new List<string>();
            bool inQuotes = false;
            var current = new System.Text.StringBuilder();

            foreach (char c in input)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    current.Append(c);
                }
                else if (c == separator && !inQuotes)
                {
                    parts.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            parts.Add(current.ToString());

            if (parts.Count != expectedParts)
                return null;

            return parts.ToArray();
        }
    }

    public class SystemRezerwacji
    {
        private readonly List<Wydarzenie> _wydarzenia = new List<Wydarzenie>();

        public IReadOnlyList<Wydarzenie> Wydarzenia => _wydarzenia.AsReadOnly();

        public (bool Sukces, string Komunikat) DodajWydarzenie(Wydarzenie wydarzenie)
        {
            if (_wydarzenia.Any(w => string.Equals(w.Nazwa, wydarzenie.Nazwa, StringComparison.OrdinalIgnoreCase)))
                return (false, "Wydarzenie o takiej nazwie już istnieje.");

            _wydarzenia.Add(wydarzenie);
            return (true, $"Dodano wydarzenie: {wydarzenie.Nazwa}.");
        }

        public string ListaWydarzenTekst()
        {
            if (_wydarzenia.Count == 0)
                return "Brak wydarzeń.";

            var lines = _wydarzenia
                .OrderBy(w => w.Data)
                .Select((w, i) => $"{i + 1}. {w.OpisTekstowy()}");
            return string.Join(Environment.NewLine, lines);
        }

        public Wydarzenie? ZnajdzPoNazwie(string nazwa)
        {
            return _wydarzenia.FirstOrDefault(w => string.Equals(w.Nazwa, nazwa, StringComparison.OrdinalIgnoreCase));
        }

        public (bool Sukces, string Komunikat) ZapiszDoPliku(string sciezka)
        {
            try
            {
                var dir = Path.GetDirectoryName(Path.GetFullPath(sciezka));
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using var sw = new StreamWriter(sciezka, false);
                sw.WriteLine("# System rezerwacji biletów - dane wydarzeń");
                sw.WriteLine("# Format: Nazwa;yyyy-MM-dd HH:mm;LiczbaMiejsc;LiczbaZarezerwowanych");
                foreach (var w in _wydarzenia.OrderBy(w => w.Data))
                {
                    sw.WriteLine(w.ToCsv());
                }
                return (true, $"Zapisano {_wydarzenia.Count} wydarzeń do pliku: {sciezka}");
            }
            catch (Exception ex)
            {
                return (false, $"Błąd zapisu: {ex.Message}");
            }
        }

        public (bool Sukces, string Komunikat, int Liczba) WczytajZPliku(string sciezka)
        {
            if (!File.Exists(sciezka))
                return (false, "Plik nie istnieje.", 0);

            try
            {
                var linie = File.ReadAllLines(sciezka);
                var nowe = new List<Wydarzenie>();

                foreach (var line in linie)
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
                        continue;

                    try
                    {
                        var w = Wydarzenie.FromCsv(trimmed);
                        int idx = nowe.FindIndex(x => string.Equals(x.Nazwa, w.Nazwa, StringComparison.OrdinalIgnoreCase));
                        if (idx >= 0) nowe[idx] = w;
                        else nowe.Add(w);
                    }
                    catch
                    {
                    }
                }

                _wydarzenia.Clear();
                _wydarzenia.AddRange(nowe.OrderBy(w => w.Data));
                return (true, $"Wczytano wydarzenia z pliku: {sciezka}", _wydarzenia.Count);
            }
            catch (Exception ex)
            {
                return (false, $"Błąd odczytu: {ex.Message}", 0);
            }
        }
    }

    class Program
    {
        private const string DomyslnyPlikDanych = "dane_wydarzenia.txt";
        private const string DomyslnyPlikLog = "log_operacji.txt";

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var system = new SystemRezerwacji();

            var wczytane = system.WczytajZPliku(DomyslnyPlikDanych);
            if (wczytane.Sukces)
            {
                Info($"Start programu. Wczytano {wczytane.Liczba} wydarzeń z {DomyslnyPlikDanych}.");
            }
            else
            {
                Info("Start programu. Brak danych lub nieudane wczytanie – zacznij od dodania wydarzeń.");
            }

            while (true)
            {
                WyswietlMenu();
                Console.Write("Wybierz opcję (1-8): ");
                var wybor = Console.ReadLine()?.Trim();

                switch (wybor)
                {
                    case "1":
                        DodajWydarzenie(system);
                        break;
                    case "2":
                        RezerwujMiejsca(system);
                        break;
                    case "3":
                        AnulujRezerwacje(system);
                        break;
                    case "4":
                        WyswietlListe(system);
                        break;
                    case "5":
                        ZapiszDoPliku(system);
                        break;
                    case "6":
                        WczytajZPliku(system);
                        break;
                    case "7":
                        PokazSzybkieTesty(system);
                        break;
                    case "8":
                        Info("Zamykanie programu...");
                        return;
                    default:
                        Komunikat("Nieznana opcja. Wybierz 1-8.");
                        break;
                }

                Console.WriteLine();
            }
        }

        static void WyswietlMenu()
        {
            Console.WriteLine("==============================================");
            Console.WriteLine(" System rezerwacji biletów - Menu główne");
            Console.WriteLine("==============================================");
            Console.WriteLine(" 1. Dodaj wydarzenie");
            Console.WriteLine(" 2. Zarezerwuj miejsca");
            Console.WriteLine(" 3. Anuluj rezerwację");
            Console.WriteLine(" 4. Wyświetl listę wydarzeń");
            Console.WriteLine(" 5. Zapisz wydarzenia do pliku (.txt)");
            Console.WriteLine(" 6. Wczytaj wydarzenia z pliku (.txt)");
            Console.WriteLine(" 7. Szybkie testy (demo)");
            Console.WriteLine(" 8. Wyjście");
            Console.WriteLine("==============================================");
        }

        static void DodajWydarzenie(SystemRezerwacji system)
        {
            Console.WriteLine("-- Dodawanie wydarzenia --");

            Console.Write("Nazwa: ");
            string? nazwa = Console.ReadLine();

            Console.Write("Data (yyyy-MM-dd HH:mm): ");
            string? dataTxt = Console.ReadLine();

            Console.Write("Liczba miejsc (int > 0): ");
            string? miejscaTxt = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(nazwa) || string.IsNullOrWhiteSpace(dataTxt) || string.IsNullOrWhiteSpace(miejscaTxt))
            {
                Komunikat("Nie podano wszystkich danych.");
                return;
            }

            if (!DateTime.TryParseExact(dataTxt.Trim(), "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
            {
                Komunikat("Nieprawidłowy format daty. Użyj: yyyy-MM-dd HH:mm");
                return;
            }

            if (!int.TryParse(miejscaTxt.Trim(), out int liczbaMiejsc) || liczbaMiejsc <= 0)
            {
                Komunikat("Nieprawidłowa liczba miejsc.");
                return;
            }

            try
            {
                var wydarzenie = new Wydarzenie(nazwa!, data, liczbaMiejsc);
                var wynik = system.DodajWydarzenie(wydarzenie);
                if (wynik.Sukces)
                {
                    Sukces(wynik.Komunikat);
                    ZapiszLog($"Dodano: {wydarzenie.OpisTekstowy()}");
                }
                else
                {
                    Komunikat(wynik.Komunikat);
                }
            }
            catch (Exception ex)
            {
                Komunikat($"Błąd dodawania: {ex.Message}");
            }
        }

        static void RezerwujMiejsca(SystemRezerwacji system)
        {
            Console.WriteLine("-- Rezerwacja miejsc --");
            Console.Write("Podaj nazwę wydarzenia: ");
            string? nazwa = Console.ReadLine();

            var wydarzenie = system.ZnajdzPoNazwie(nazwa ?? "");
            if (wydarzenie == null)
            {
                Komunikat("Nie znaleziono wydarzenia o podanej nazwie.");
                return;
            }

            Console.Write($"Ile miejsc zarezerwować (wolnych: {wydarzenie.LiczbaWolnychMiejsc()}): ");
            string? ileTxt = Console.ReadLine();
            if (!int.TryParse(ileTxt?.Trim(), out int ile))
            {
                Komunikat("Nieprawidłowa liczba.");
                return;
            }

            var wynik = wydarzenie.Zarezerwuj(ile);
            if (wynik.Sukces)
            {
                Sukces(wynik.Komunikat);
                ZapiszLog($"Rezerwacja: {wydarzenie.Nazwa} +{ile} -> Zarezerwowane: {wydarzenie.LiczbaZarezerwowanych}");
            }
            else
            {
                Komunikat(wynik.Komunikat);
            }
        }

        static void AnulujRezerwacje(SystemRezerwacji system)
        {
            Console.WriteLine("-- Anulowanie rezerwacji --");
            Console.Write("Podaj nazwę wydarzenia: ");
            string? nazwa = Console.ReadLine();

            var wydarzenie = system.ZnajdzPoNazwie(nazwa ?? "");
            if (wydarzenie == null)
            {
                Komunikat("Nie znaleziono wydarzenia o podanej nazwie.");
                return;
            }

            Console.Write($"Ile miejsc anulować (zarezerwowane: {wydarzenie.LiczbaZarezerwowanych}): ");
            string? ileTxt = Console.ReadLine();
            if (!int.TryParse(ileTxt?.Trim(), out int ile))
            {
                Komunikat("Nieprawidłowa liczba.");
                return;
            }

            var wynik = wydarzenie.Anuluj(ile);
            if (wynik.Sukces)
            {
                Sukces(wynik.Komunikat);
                ZapiszLog($"Anulacja: {wydarzenie.Nazwa} -{ile} -> Zarezerwowane: {wydarzenie.LiczbaZarezerwowanych}");
            }
            else
            {
                Komunikat(wynik.Komunikat);
            }
        }

        static void WyswietlListe(SystemRezerwacji system)
        {
            Console.WriteLine("-- Lista wydarzeń --");
            Console.WriteLine(system.ListaWydarzenTekst());
        }

        static void ZapiszDoPliku(SystemRezerwacji system)
        {
            Console.WriteLine("-- Zapis do pliku --");
            Console.Write($"Ścieżka pliku (ENTER = {DomyslnyPlikDanych}): ");
            string? sciezka = Console.ReadLine();
            sciezka = string.IsNullOrWhiteSpace(sciezka) ? DomyslnyPlikDanych : sciezka.Trim();

            var wynik = system.ZapiszDoPliku(sciezka);
            if (wynik.Sukces)
            {
                Sukces(wynik.Komunikat);
                ZapiszLog($"Zapis do pliku: {sciezka}");
            }
            else
            {
                Komunikat(wynik.Komunikat);
            }
        }

        static void WczytajZPliku(SystemRezerwacji system)
        {
            Console.WriteLine("-- Wczytanie z pliku --");
            Console.Write($"Ścieżka pliku (ENTER = {DomyslnyPlikDanych}): ");
            string? sciezka = Console.ReadLine();
            sciezka = string.IsNullOrWhiteSpace(sciezka) ? DomyslnyPlikDanych : sciezka.Trim();

            var wynik = system.WczytajZPliku(sciezka);
            if (wynik.Sukces)
            {
                Sukces($"{wynik.Komunikat}. Wczytano: {wynik.Liczba}.");
                ZapiszLog($"Wczytanie z pliku: {sciezka} (liczba: {wynik.Liczba})");
            }
            else
            {
                Komunikat(wynik.Komunikat);
            }
        }

        static void PokazSzybkieTesty(SystemRezerwacji system)
        {
            Console.WriteLine("-- Szybkie testy (demo) --");

            var r1 = system.DodajWydarzenie(new Wydarzenie("Koncert Jesienny", DateTime.Now.AddDays(7).Date.AddHours(19), 100));
            Console.WriteLine(r1.Sukces ? "Test 1 OK: Dodano 'Koncert Jesienny'." : $"Test 1 INFO: {r1.Komunikat}");

            var w1 = system.ZnajdzPoNazwie("Koncert Jesienny");
            if (w1 != null)
            {
                var r2 = w1.Zarezerwuj(20);
                Console.WriteLine(r2.Sukces ? "Test 2 OK: Zarezerwowano 20." : $"Test 2 FAIL: {r2.Komunikat}");

                var r3 = w1.Zarezerwuj(1000);
                Console.WriteLine(!r3.Sukces && r3.Komunikat.Contains("Brak") ? "Test 3 OK: Nadmierna rezerwacja odrzucona." : $"Test 3 FAIL: {r3.Komunikat}");

                var r4 = w1.Anuluj(5);
                Console.WriteLine(r4.Sukces ? "Test 4 OK: Anulowano 5." : $"Test 4 FAIL: {r4.Komunikat}");

                var r5 = w1.Anuluj(1000);
                Console.WriteLine(!r5.Sukces && r5.Komunikat.Contains("Nie można") ? "Test 5 OK: Nadmierna anulacja odrzucona." : $"Test 5 FAIL: {r5.Komunikat}");
            }

            var r6 = system.ZapiszDoPliku(DomyslnyPlikDanych);
            Console.WriteLine(r6.Sukces ? "Test 6 OK: Zapisano plik." : $"Test 6 FAIL: {r6.Komunikat}");

            var r7 = system.WczytajZPliku(DomyslnyPlikDanych);
            Console.WriteLine(r7.Sukces ? $"Test 7 OK: Wczytano {r7.Liczba}." : $"Test 7 FAIL: {r7.Komunikat}");

            Console.WriteLine("Test 8: Lista wydarzeń:");
            Console.WriteLine(system.ListaWydarzenTekst());
        }

        static void Komunikat(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[INFO] {msg}");
            Console.ResetColor();
            ZapiszLog($"INFO: {msg}");
        }

        static void Sukces(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[OK] {msg}");
            Console.ResetColor();
            ZapiszLog($"OK: {msg}");
        }

        static void Info(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[SYSTEM] {msg}");
            Console.ResetColor();
            ZapiszLog($"SYSTEM: {msg}");
        }

        static void ZapiszLog(string tresc)
        {
            try
            {
                using var sw = new StreamWriter(DomyslnyPlikLog, append: true);
                sw.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {tresc}");
            }
            catch
            {
            }
        }
    }
}
