class Tank : BaseObject
{
    // ... other properties and methods

    private List<Bullet> bulletsList; // Renamed to bulletsList to avoid ambiguity

    public List<Bullet> Bullets
    {
        get
        {
            return bulletsList;
        }

        set
        {
            bulletsList = value;
        }
    }

    // ... other properties and methods
}
