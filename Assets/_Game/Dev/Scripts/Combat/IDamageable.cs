namespace SexShot.Dev.Combat
{
    public interface IDamageable
    {
        DamageTeam Team { get; }
        bool IsAlive { get; }
        void TakeDamage(float amount, DamageTeam sourceTeam);
    }
}
