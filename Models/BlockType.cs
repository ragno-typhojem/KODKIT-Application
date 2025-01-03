namespace KodKit.Models
{
    public enum BlockType
    {
        // Hareket Blokları
        MoveSteps,           // ... adım git
        TurnRight,           // ... derece sağa dön
        TurnLeft,           // ... derece sola dön
        GoToPosition,       // x: ... y: ... konumuna git
        SetXPosition,       // x'i ... yap
        SetYPosition,       // y'yi ... yap

        // Görünüm Blokları
        SayMessage,         // ... de
        ShowSprite,         // göster
        HideSprite,        // gizle
        ChangeCostume,      // sonraki kostüme geç

        // Ses Blokları
        PlaySound,          // ... sesini çal
        StopAllSounds,      // tüm sesleri durdur
        PlayNote,           // nota çal
        PlayDrum,          // davul çal

        // Olay Blokları
        WhenFlagClicked,    // yeşil bayrak tıklandığında
        WhenKeyPressed,     // ... tuşu basıldığında
        WhenSpriteClicked,  // bu kukla tıklandığında

        // Kontrol Blokları
        Wait,               // ... saniye bekle
        Repeat,            // ... kez tekrarla
        Forever,           // sürekli tekrarla
        IfThen,           // eğer ... ise
        WaitUntil,        // ... olana kadar bekle

        // Değişken Blokları
        SetVariable,        // ... değişkenini ... yap
        ChangeVariable,     // ... değişkenini ... kadar değiştir
        ShowVariable,       // değişkeni göster
        HideVariable       // değişkeni gizle
    }
}
