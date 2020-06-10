namespace AAEmu.Game.Models.Game.Items.Procs
{
    public enum ProcChanceKind
    {
        HitAny = 0x1,
        HitMelee = 0x2,
        HitMeleeCrit = 0x3,
        HitSpell = 0x4,
        HitSpellCrit = 0x5,
        HitRanged = 0x6,
        HitRangedCrit = 0x7,
        HitSiege = 0x8,
        TakeDamageAny = 0x9,
        TakeDamageMelee = 0xA,
        TakeDamageMeleeCrit = 0xB,
        TakeDamageSpell = 0xC,
        TakeDamageSpellCrit = 0xD,
        TakeDamageRanged = 0xE,
        TakeDamageRangedCrit = 0xF,
        TakeDamageSiege = 0x10,
        HitHeal = 0x11,
        HitHealCrit = 0x12,
        FireSkill = 0x13,
        HitSkill = 0x14,
    }
}
