using UnityEngine;
using System;
using System.Collections.Generic;
using OgreMaths;

namespace OgresLair.Game
{
    public abstract class Entity
    {
        public Point2 m_position = new Point2();
        public float m_rotation;
        public float m_health;
  
        public TileType m_associatedTile;

        public static List<Entity> m_listAll = new List<Entity>();

        public Entity()
        {
            m_listAll.Add(this);
        }

        public bool IsAlive()
        {
            return m_health > 0;
        }

        public static void Add(Entity e)
        {
            m_listAll.Add(e);
        }

        public static void clearAll()
        {
            m_listAll.Clear();
        }

        public bool IsFacing(Entity to)
        {
            float cos = (float)Math.Cos(m_rotation);
            float sin = (float)Math.Sin(m_rotation);

            Vector2 a = new Vector2(sin, -cos);
            Vector2 b = new Vector2(to.m_position.X - m_position.X, to.m_position.Y - m_position.Y);
            
            a.Normalize();
            b.Normalize();

            return Vector2.Dot(a, b) > 0;
        }

        public float getAngleTo(Entity to)
        {
            Vector2 vector = new Vector2(
                to.m_position.X - m_position.X,
                to.m_position.Y - m_position.Y);

            return getAngleOfVector(vector);
        }

        public float getAngleOfVector(Vector2 vector)
        {
            Vector2 newVector = new Vector2(vector.x, -vector.y);
            newVector.Normalize();

            float angle = (float)Math.Acos(newVector.y);
            if (newVector.x < 0.0f)
            {
                angle = -angle;
                angle += Angles.DEG_360;
            }

            if (float.IsNaN(angle))
            {
                return 0.0f;
            }

            return angle;
        }

        public int distanceSquaredFrom(Entity to)
        {
            int deltaX = to.m_position.X - m_position.X;
            int deltaY = to.m_position.Y - m_position.Y;

            return deltaX * deltaX + deltaY * deltaY;
        }

        public int distanceSquaredFrom(Point2 to)
        {
            int deltaX = to.X - m_position.X;
            int deltaY = to.Y - m_position.Y;

            return deltaX * deltaX + deltaY * deltaY;
        }

        public bool applyDamageOneHit(float _hitAmount)
        {
            return applyDamage(_hitAmount);
        }

        public bool applyDamageDrain(float frameTime, float _hitAmountPerSec)
        {
            return applyDamage(_hitAmountPerSec * frameTime);
        }

        virtual protected bool applyDamage(float damage)
        {
            m_health -= damage;
            if (m_health <= 0)
            {
                m_health = 0;
                death();
            }

            NotifyApplyDamage(damage);
            return m_health < 0;
        }

        public virtual void NotifyApplyDamage(float damage) { }
        abstract public void death();
    }

}
