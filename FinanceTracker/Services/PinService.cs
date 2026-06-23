using FinanceTracker.Model;
using FinanceTracker.Store;

namespace FinanceTracker.Services;

/// <summary>Служба авторизации по PIN-коду. Демонстрационный код: 0000.</summary>
public sealed class PinService
{
    private readonly FinanceDb _db;
    public Profile? Active { get; private set; }

    public PinService(FinanceDb db) => _db = db;

    public bool Enter(string pin)
    {
        var profile = _db.Profiles.Rows.FirstOrDefault();
        if (profile is null) return false;

        if (Hashing.Matches(pin, profile.PinHash))
        {
            Active = profile;
            Journal.Note($"Вход в приложение: {profile.Owner}");
            return true;
        }
        Journal.Note("Неудачная попытка входа");
        return false;
    }

    public void Leave()
    {
        if (Active is not null)
            Journal.Note($"Выход: {Active.Owner}");
        Active = null;
    }
}
