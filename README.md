#  MovieApp – Rekomendacje filmowe oparte na emocjach

MovieApp to aplikacja w języku C# wykorzystująca **ML.NET** i SQLite do rekomendowania filmów użytkownikom na podstawie analizy emocji w recenzjach. System łączy dane o ocenach użytkowników, emocjonalne profile filmów oraz teksty recenzji, aby dostarczyć spersonalizowane rekomendacje.

---

##  Funkcjonalności

- Pobieranie danych o filmach i recenzjach z **The Movie Database API (TMDb)**.
- Analiza emocji w recenzjach użytkowników przy użyciu wytrenowanego modelu ML.NET.
- Tworzenie profilu emocjonalnego filmów oraz użytkowników.
- Obliczanie rekomendacji filmowych na podstawie podobieństwa profili emocjonalnych.
- Obsługa ocen użytkowników oraz filtrowanie filmów po gatunkach i tytule.
- Wyświetlanie top rekomendacji w interfejsie aplikacji.

---

##  Model emocji

Model analizy emocji rozpoznaje sześć klas emocji w recenzjach:

- sadness  
- anger  
- love  
- surprise  
- fear  
- joy  

Model (`emotion_model.zip`) **został wytrenowany w projekcie [emocje](https://github.com/rogutmichal/emocje)** i jest wczytywany w tej aplikacji przy użyciu ML.NET PredictionEngine do klasyfikacji tekstów recenzji.

**Do treningu modelu użyto zbioru danych:** [Emotions Dataset for NLP na Kaggle](https://www.kaggle.com/datasets/praveengovi/emotions-dataset-for-nlp).

---

##  Struktura bazy danych

Baza danych SQLite przechowuje:

- **Movie** – dane o filmach (tytuł, opis, gatunek, data premiery, popularność, ścieżka do plakatu).  
- **Review** – recenzje filmów wraz z top 6 emocjami i wynikami predykcji.  
- **User** – dane o użytkownikach (ID, nazwa, rola).  
- **Rating** – oceny filmów przypisane do użytkowników.

---

##  Proces rekomendacji

1. Pobranie wszystkich filmów, recenzji i ocen użytkownika z bazy danych.
2. Analiza recenzji i przypisanie wartości emocjonalnych do każdego filmu.
3. Obliczenie profilu emocjonalnego użytkownika na podstawie ocen i emocji filmów.
4. Obliczenie wag dla poszczególnych emocji w rekomendacjach.
5. Porównanie profilu użytkownika z profilem emocjonalnym filmów za pomocą **similarity score (Cosine Similarity)**.
6. Wyświetlenie top rekomendowanych filmów w interfejsie.

---

##  Technologie

- **C# / .NET MAUI** – interfejs użytkownika i logika aplikacji.
- **ML.NET** – analiza emocji w recenzjach.
- **SQLite** – lokalna baza danych filmów, recenzji, użytkowników i ocen.
- **TMDb API** – pobieranie informacji o filmach i recenzjach.
- **Newtonsoft.Json** – przetwarzanie danych JSON z API filmów.

---

##  Uruchomienie

1. Skonfiguruj plik `appsettings.json` z kluczem API do TMDb:
```json
{
  "ApiKeys": {
    "MovieDb": "TWÓJ_KLUCZ_API"
  }
}
