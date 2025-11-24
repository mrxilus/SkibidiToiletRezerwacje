# System rezerwacji biletów

## Opis projektu
Projekt to konsolowa aplikacja w języku **C#**, która umożliwia obsługę prostego systemu rezerwacji biletów na wydarzenia.  
Program pozwala na:
- dodawanie wydarzeń,
- rezerwowanie miejsc,
- anulowanie rezerwacji,
- wyświetlanie listy wydarzeń,
- zapisywanie i wczytywanie danych z pliku `.txt`.

Dane przechowywane są w prostym formacie CSV (pola oddzielone średnikiem).

---

## Instrukcja uruchomienia programu
1. Wymagany jest zainstalowany **.NET SDK** (np. .NET 6 lub nowszy).
2. Utwórz nowy projekt konsolowy:
   ```bash
   dotnet new console -n SystemRezerwacjiBiletow
3. Zamień zawartość pliku Program.cs na kod źródłowy programu.
4. ```bash
   dotnet run
5. Program automatycznie utworzy pliki:
   - `dane_wydarzenia.txt` – zapis i odczyt listy wydarzeń,
   - `log_operacji.txt` – logi działania programu.

## Struktura projektu
### Klasa `Wydarzenie`
Reprezentuje pojedyncze wydarzenie.
#### Pola i właściwości:
 - Nazwa – nazwa wydarzenia,
 - Data – data i godzina wydarzenia,
 - LiczbaMiejsc – całkowita liczba miejsc,
 - LiczbaZarezerwowanych – liczba już zarezerwowanych miejsc.
#### Metody:
 - Zarezerwuj(int ile) – rezerwuje określoną liczbę miejsc, jeśli są dostępne.
 - Anuluj(int ile) – anuluje określoną liczbę miejsc, jeśli rezerwacje istnieją.
 - LiczbaWolnychMiejsc() – zwraca liczbę wolnych miejsc.
 - OpisTekstowy() – zwraca informacje o wydarzeniu w postaci tekstowej.
 - ToCsv() – konwertuje dane wydarzenia do formatu CSV.
 - FromCsv(string line) – odczytuje wydarzenie z linii CSV.

### Klasa `SystemRezerwacji`
Zarządza kolekcją obiektów typu `Wydarzenie`.
#### Metody:
 - DodajWydarzenie(Wydarzenie) – dodaje nowe wydarzenie.
 - ListaWydarzenTekst() – zwraca listę wydarzeń w formie tekstowej.
 - ZnajdzPoNazwie(string) – wyszukuje wydarzenie po nazwie.
 - ZapiszDoPliku(string) – zapisuje listę wydarzeń do pliku `.txt`.
 - WczytajZPliku(string) – wczytuje wydarzenia z pliku `.txt`.

### Klasa `Program`
#### Zawiera logikę interfejsu konsolowego:
 - menu główne,
 - obsługę wejścia użytkownika,
 - komunikaty zwrotne,
 - logowanie operacji do pliku.

## Przykładowe użycie programu
1. Dodanie wydarzenia:
    ```Kod
    Nazwa: Koncert Metalica
    Data: 2025-11-15 19:00
    Liczba miejsc: 100
Wynik: wydarzenie pojawia się na liście.

2. Rezerwacja miejsc:
    ```Kod
    Podaj nazwę wydarzenia: Koncert Metalica
    Ile miejsc zarezerwować: 20
Wynik: liczba zarezerwowanych wzrasta.  

3. Nadmierna rezerwacja:
    ```Kod
    Podaj nazwę wydarzenia: Koncert Metalica
    Ile miejsc zarezerwować: 200
Wynik: komunikat „Brak wystarczającej liczby miejsc”.  

4. Anulowanie rezerwacji:
    ```Kod
    Podaj nazwę wydarzenia: Koncert Metalica
    Ile miejsc anulować: 5
Wynik: liczba zarezerwowanych spada.  

5. Anulowanie zbyt wielu miejsc:
    ```Kod
    Podaj nazwę wydarzenia: Koncert Metalica
    Ile miejsc anulować: 100
Wynik: komunikat „Nie można anulować więcej miejsc niż zarezerwowano”.  

6. Zapis do pliku:
    ```Kod
    Ścieżka pliku: dane_wydarzenia.txt
Wynik: plik istnieje i zawiera poprawne dane.  

7. Wczytanie z pliku:
    ```Kod
    Ścieżka pliku: dane_wydarzenia.txt
Wynik: wczytane wydarzenia pojawiają się w liście.  

8. Wyświetlenie listy:
    ```Kod
    1. Koncert Metalica | 2025-11-15 19:00 | Miejsca: 100 | Zarezerwowane: 15 | Wolne: 85

## Zakres testów
 - Dodanie wydarzenia – ✅
 - Rezerwacja miejsc – ✅
 - Nadmierna rezerwacja – ✅
 - Anulowanie rezerwacji – ✅
 - Anulowanie zbyt wielu miejsc – ✅
 - Zapis do pliku – ✅
 - Wczytanie z pliku – ✅
 - Wyświetlenie listy – ✅

## Autor
- Imię i nazwisko: Mikołaj Legień
- Data wykonania zadania: 03.11.2025
- Ostatnia aktualizacja została wykonana 17.11.2025 słownie: siedemnastego listopada dwa tysiące dwadzieścia pięć o godzinie 13:49 słownie: trzynastej czterdzieści dziewięć roku pańskiego 2025 (anno domini) czasu UTC+2, Last update was made on 17.11.2025 at 13:49 (anno domini)
