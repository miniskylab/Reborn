namespace Reborn.Common
{
    public class Resources
    {
        float _mass, _energy, _maxMass, _maxEnergy, _massSurplus, _energySurplus;

        public float Energy
        {
            get { return _energy; }
            set
            {
                if (value < 0) value = 0;
                if (value > _maxEnergy) value = _maxEnergy;
                _energy = value;
                OnChanged?.Invoke();
            }
        }
        public float EnergySurplus
        {
            get { return _energySurplus; }
            set
            {
                _energySurplus = value;
                OnChanged?.Invoke();
            }
        }
        public float Mass
        {
            get { return _mass; }
            set
            {
                if (value < 0) value = 0;
                if (value > _maxMass) value = _maxMass;
                _mass = value;
                OnChanged?.Invoke();
            }
        }
        public float MassSurplus
        {
            get { return _massSurplus; }
            set
            {
                _massSurplus = value;
                OnChanged?.Invoke();
            }
        }
        public float MaxEnergy
        {
            get { return _maxEnergy; }
            set
            {
                if (value < 0) value = 0;
                if (value > 99999) value = 99999;
                _maxEnergy = value;
                OnChanged?.Invoke();
            }
        }
        public float MaxMass
        {
            get { return _maxMass; }
            set
            {
                if (value < 0) value = 0;
                if (value > 99999) value = 99999;
                _maxMass = value;
                OnChanged?.Invoke();
            }
        }

        public event Events.Empty OnChanged;
    }
}