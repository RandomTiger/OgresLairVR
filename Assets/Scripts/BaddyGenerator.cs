using System;
using UnityEngine;
using Audio;
#if false
namespace OgresLair.Game
{
    class Baddy : Character
    {
        public float m_age; 
        public bool m_inGenerator;

        public Baddy()
            : base((int)CharacterTile.BADDY, TileType.NONE)
        {
            Reset();               
        }

        public void Reset()
        {
            m_age = 0;
            m_inGenerator = true;
        }
        override public void death()
        {
            m_health = 0.0f;
            m_isActive = false;
            Reset();

            Sound.Play("SFX_Imp_Death");
        }

        protected override void stopMovement()
        {

        }

        public void randomlyAssignDirection()
        {
            float mult = (float) UnityEngine.Random.Range(0, 4);
            m_rotation = (float) Math.PI * mult / 2;
        }

        public override TileType SetInventory(TileType item) { return TileType.NONE;  }
    }

    public class BaddyGenerator
    {
        Point2 m_pixelPos;
        Baddy [] m_baddyArray;

        int m_growingIndex;

        public BaddyGenerator(Map _map, Point2 mapPos)
        {
            m_growingIndex = -1;

            m_pixelPos = new Point2(
                mapPos.X * _map.getTileWidth()  + _map.getTileWidth() / 2,
                mapPos.Y * _map.getTileWidth() + _map.getTileWidth() / 2);

            m_baddyArray = new Baddy[GameConstants.baddyGeneratorMaxBaddies];
            for (int i = 0; i < m_baddyArray.Length; i++)
            {
                m_baddyArray[i] = new Baddy();
                m_baddyArray[i].SetStartPosition(m_pixelPos);
            }
        }

        public void update(float frameTime, Map _map, Character[] _characterArray)
        {
            if (m_growingIndex == -1)
            {
                for (int i = 0; i < m_baddyArray.Length; i++)
                {
                    if (m_baddyArray[i].m_inGenerator)
                    {
                        m_baddyArray[i].m_position = m_pixelPos;
                        m_growingIndex = i;
                        break;
                    }
                }
            }

            if (m_growingIndex != -1)
            {

                m_baddyArray[m_growingIndex].m_age += frameTime / GameConstants.baddyGenerateTime;
                if (m_baddyArray[m_growingIndex].m_age >= 1.0)
                {
                    m_baddyArray[m_growingIndex].m_health = GameConstants.baddyInitialHealth;
                    m_baddyArray[m_growingIndex].m_age = 1.0f;
                    m_baddyArray[m_growingIndex].m_inGenerator = false;
                    m_baddyArray[m_growingIndex].randomlyAssignDirection();
                    m_growingIndex = -1;
                }

            }

            for(int i = 0; i < m_baddyArray.Length; i++)
            {                                       
                if (m_baddyArray[i].IsAlive())
                {
                    if (m_baddyArray[i].movePosition(frameTime, _map, _characterArray, false, false))
                    {
                        m_baddyArray[i].randomlyAssignDirection();
                    }
                }               
            }
        }

        public bool isAnyBaddieOnTile(Point2 tile)
        {
            for (int i = 0; i < m_baddyArray.Length; i++)
            {
                if (m_baddyArray[i].isCharacterOnTile(tile))
                {
                    return true;
                }
            }

            return false;
        }

        public int KillAllInRadius(Point2 position, float distance)
        {
            int countKilled = 0;

            for (int i = 0; i < m_baddyArray.Length; i++)
            {
                if(m_baddyArray[i].IsAlive() && m_baddyArray[i].distanceSquaredFrom(position) < distance * distance)
                {
                    m_baddyArray[i].death();
                    countKilled++;

                    if (i == m_growingIndex)
                    {
                        m_growingIndex = -1;
                    }
                }
            }

            return countKilled;
        }
    }
}
#endif