using System;
using UnityEngine;
using Audio;
#if false
namespace OgresLair.Game
{
    // Enumeration of the tiles in characters.tga
    public enum CharacterTile
        {
            WARRIOR,    
            WIZARD,
            WARRIOR_LEGS,
            WIZARD_LEGS,
            BADDY,
            WARRIOR_FIRE,
            WIZARD_FIRE,
            TURRET_FIRE,
            DEAD,
            OGRE,
            SWORD_FIRE,
            MAX,

            DEFAULT = -1,
        };

    public abstract class Character : Entity
    {
        // Rendering
        public int m_score;

        public int m_nextLevel;
        public bool m_finishedLevel;
        public bool m_isActive;

        protected int m_frameIndexRendering;
        protected int m_frameIndexCollision;
        protected float m_scale;
        protected float m_moveSpeedPerSecond = 500;
        protected float MAX_VELOCITY = 500;
      
        // current state
  
        // Collision in 2D by all z = 0
        BoundingSphere m_boundingCharCircle = new BoundingSphere();
        BoundingBox m_boundingTileBox = new BoundingBox();
        BoundingSphere m_boundingTileCircle = new BoundingSphere();

        // Velocity currently only used for sliding
        Vector2 m_velocity = new Vector2();
        protected TileType m_inventory = TileType.NONE;
        protected int m_characterRadius;

        public Character()
        {
            ResetBetweenLevels();
        }

        public void ResetBetweenLevels()
        {
            //m_teleporting = false;
            m_finishedLevel = false;
            m_scale = 1.0f;
            m_isActive = true;
            m_velocity = new Vector2();
        }

        public void SetStartPosition(Point2 startPos)
        {
            m_position = new Point2(startPos);
        }

        public static int capValue(int _value, int _min, int _max)
        {
            return Math.Min(Math.Max(_value, _min), _max);
        }

        protected bool moveBolder(Map _theMap, int _pixelX, int _pixelY)
        {
            // First find out where we want to push the block to
            int tileX = _theMap.getTileNumFromPixelX(_pixelX);
            int tileY = _theMap.getTileNumFromPixelY(_pixelY);

            int newTileX = tileX;
            int newTileY = tileY;
           
            int diffX = _pixelX - m_position.X;
            int diffY = _pixelY - m_position.Y;

            float angle = 20.0f * Angles.DEG_1;
            float rotationMod = m_rotation;

            while (rotationMod > Angles.DEG_45)
            {
                rotationMod -= Angles.DEG_90;
            }

            if (Math.Abs(rotationMod) < angle)
            {
                if (Math.Abs(diffX) > Math.Abs(diffY))
                {
                    newTileX = tileX + diffX / Math.Abs(diffX);
                }
                else
                {
                    newTileY = tileY + diffY / Math.Abs(diffY);
                }
            }
            else
            {
                if (Math.Abs(diffX) > Math.Abs(diffY))
                {
                    if (Math.Abs(diffY) != 0)
                    {
                        newTileY = tileY + diffY / Math.Abs(diffY);
                    }
                }
                else
                {
                    if (Math.Abs(diffX) != 0)
                    {
                        newTileX = tileX + diffX / Math.Abs(diffX);
                    }
                   
                }
            }


            // Secondly now we need to check if the way if blocked
            TileType destination = _theMap.getMapTileType(newTileX, newTileY);
            if(destination == TileType.FLOOR)
            {
                if (!isAnyCharacterOnTile(_theMap, new Point2(newTileX, newTileY)))
                {
                    _theMap.setMapTileType(tileX, tileY, TileType.FLOOR);
                    _theMap.setMapTileType(newTileX, newTileY, TileType.USABLE_BOLDER);
                    return true;
                }
            }

            return false;
        }

        bool isAnyCharacterOnTile(Map _theMap, Point2 tilePos)
        {
            for(int i = 0; i < _theMap.m_baddyArray.Count; i++)
            {
                if (((BaddyGenerator)_theMap.m_baddyArray[i]).isAnyBaddieOnTile(tilePos))
                {
                    return true;
                }
            }

            for(int c = 0; c < Game.m_characters.Length; c++)
            {
                if (Game.m_characters[c].isCharacterOnTile(tilePos))
                {
                    return true;
                }
            }

            for (int p = 0; p < Game.m_npcs.Length; p++)
            {
                if (Game.m_npcs[p].isCharacterOnTile(tilePos))
                {
                    return true;
                }
            }

            return false;
        }

        protected TileType pickup(TileType _item)
        { 
            bool swap = false;
            TileType putDown = TileType.FLOOR;

            if (Map.IsKey(_item))
            {
                Sound.Play("SFX_Pickup_Key");
                swap = true;
            }
            else
            {

                switch (_item)
                {
                    case TileType.PICKUP_GEM_BLUE:
                        increaseScore(GameConstants.pickupGemBlueScoreChange);
                        Sound.Play("SFX_Pickup_Jewel");
                        break;
                    case TileType.PICKUP_GEM_GREEN:
                        increaseScore(GameConstants.pickupGemGreenScoreChange);
                        Sound.Play("SFX_Pickup_Jewel");
                        break;
                    case TileType.PICKUP_GEM_PINK:
                        increaseScore(GameConstants.pickupGemPinkScoreChange);
                        Sound.Play("SFX_Pickup_Jewel");
                        break;
                    case TileType.PICKUP_FIRE_POWER:
                        increaseFirePower();
                        Sound.Play("SFX_Pickup_Mushroom");
                        break;
                    case TileType.PICKUP_FIRE_SPEED:
                        increaseFireFequency();
                        Sound.Play("SFX_Pickup_Mushroom");
                        break;
                    case TileType.PICKUP_SHEILD:
                        increaseDefence();
                        Sound.Play("SFX_Pickup_Mushroom");
                        break;
                    case TileType.PICKUP_POTION:
                        if (m_health < 20)
                        {
                            m_health = 90;
                        }
                        else
                        {
                            applyDamage(m_health - 10);
                        }
                        Sound.Play("SFX_Pickup_Potion");
                        break;
                    case TileType.PICKUP_FOOD:
                        increaseHealth(GameConstants.pickupFoodHealthChange);
                        Sound.Play("SFX_Pickup_Food");
                        break;
                    
                    case TileType.PICKUP_CROSS:
                        Sound.Play("SFX_Pickup_Cross");
                        swap = true;
                        break;
                    case TileType.PICKUP_SWORD:
                        Sound.Play("SFX_Pickup_Sword");
                        swap = true;
                        break;
                    case TileType.PICKUP_MUSHROOM_GOOD:
                    case TileType.PICKUP_MUSHROOM_BAD:
                        Sound.Play("SFX_Pickup_Mushroom");
                        break;
                }
            }
          
            if(swap)
            {
                putDown = SetInventory(_item); 
                    
            }

            //Sound.Play("Pop");
            return putDown;
        }

        public bool movePosition(float frameTime, Map _theMap, Character[] _characterArray, bool slideAgainstWalls, bool canUseSlides)
        {
            bool movingOnSlide = false;

            bool slideAgainstDiagonalWalls = slideAgainstWalls && true;
            bool environmentCollision = false;

            // todo this shouldnt be assigned here each frame
            m_characterRadius = m_tileSet.getTileWidth() / 2 * 8 / 10;

            Point2 change = new Point2();

            int tileIndexX = m_position.X / _theMap.getTileWidth();
            int tileIndexY = m_position.Y / _theMap.getTileHeight();
            TileType tileTypeStoodOn = _theMap.getMapTileType(tileIndexX, tileIndexY);


            Boolean onSlide = Map.isTileTypeSlide(tileTypeStoodOn) && canUseSlides;
            int SLIDE_EXIT_LENGTH = 10;
            // If not sliding (on slide exit) then allow movement
            if (!onSlide)
            {
                float moveSpeed = m_moveSpeedPerSecond * frameTime;

                float cos = (float)Math.Cos(m_rotation);
                float sin = (float)Math.Sin(m_rotation);

                change.X = (int)( sin * moveSpeed);
                change.Y = (int)(-cos * moveSpeed);
            }

            Point2 newPos = new Point2();

            // Velocity is only for slide movement
            if (m_velocity.Length() > MAX_VELOCITY)
            {
                m_velocity.Normalize();
                m_velocity *= MAX_VELOCITY;
            }

            change.X += (int)(m_velocity.X * frameTime);
            newPos.X = m_position.X + change.X;

            change.Y += (int)(m_velocity.Y * frameTime);
            newPos.Y = m_position.Y + change.Y;

            if (Map.isTileTypeSlide(tileTypeStoodOn) && canUseSlides)
            {       
                Vector2 targetPoint = new Vector2();
                Vector2 charPoint = new Vector2(m_position.X, m_position.Y);
                targetPoint.X = tileIndexX * _theMap.getTileWidth() + m_tileSet.getTileWidth() / 2;
                targetPoint.Y = tileIndexY * _theMap.getTileHeight() + m_tileSet.getTileHeight() / 2;

                if (tileTypeStoodOn == TileType.SLIDE_DOWN)
                {
                    targetPoint.Y += m_tileSet.getTileHeight();
                }
                else if (tileTypeStoodOn == TileType.SLIDE_UP)
                {
                    targetPoint.Y -= m_tileSet.getTileHeight();
                }
                else if (tileTypeStoodOn == TileType.SLIDE_LEFT)
                {
                    targetPoint.X -= m_tileSet.getTileWidth();
                }
                else if (tileTypeStoodOn == TileType.SLIDE_RIGHT)
                {
                    targetPoint.X += m_tileSet.getTileWidth();
                }

                Vector2 vector = targetPoint - charPoint;

                // Its important this is high to keep the player in the center of the slide
                // So they dont come out off-center and get stuck in collision
                const float VELOCITY_INCREASE = 200.0f;
                m_velocity += vector * VELOCITY_INCREASE * frameTime;
                movingOnSlide = true;
            }
            else if (m_velocity.Length() > 0)
            {
                // dubious code to reduce velocity
                m_velocity *= (25 * frameTime);

                change.X += (int)(m_velocity.X * frameTime);
                change.Y += (int)(m_velocity.Y * frameTime);
                newPos.X = m_position.X + change.X;
                newPos.Y = m_position.Y + change.Y;
                if (m_velocity.Length() < SLIDE_EXIT_LENGTH)
                {
                    m_velocity.X = 0;
                    m_velocity.Y = 0;
                    stopMovement();
                }
            }
            else
            {
                newPos.X = m_position.X + change.X;
                newPos.Y = m_position.Y + change.Y;
            }

            Point2 lastPos = new Point2(newPos);

            // check per tile here
            if (Map.isTileTypeSlide(tileTypeStoodOn) == false && BuildConstants.COLLISION_OFF == false)// && onSlide == false)// && m_teleporting == false)
            {
                // Check against the very boundarys of the map
                newPos.X = capValue(newPos.X, m_characterRadius, _theMap.getWidth() - m_characterRadius);
                newPos.Y = capValue(newPos.Y, m_characterRadius, _theMap.getHeight() - m_characterRadius);

                if (lastPos.X != newPos.X || lastPos.Y != newPos.Y)
                {
                    environmentCollision = true;
                }

                int minX = capValue(tileIndexX - 1, 0, _theMap.getWidth() / _theMap.getTileWidth());
                int maxX = capValue(tileIndexX + 2, 0, _theMap.getWidth() / _theMap.getTileWidth());

                int minY = capValue(tileIndexY - 1, 0, _theMap.getHeight() / _theMap.getTileHeight());
                int maxY = capValue(tileIndexY + 2, 0, _theMap.getHeight() / _theMap.getTileHeight());

                m_boundingCharCircle.Center = new Vector3(newPos.X, newPos.Y, 0);
                m_boundingCharCircle.Radius = m_characterRadius;

                for (int y = minY; y < maxY; y++)
                {
                    for (int x = minX; x < maxX; x++)
                    {
                        if (x >= _theMap.getWidth() / _theMap.getTileWidth() &&
                            y >= _theMap.getHeight() / _theMap.getTileHeight())
                        {
                                 continue;
                        }

                        TileType tileType = _theMap.getMapTileType(x, y);

                        Point2 pixel = new Point2(
                            x * _theMap.getTileWidth(),
                            y * _theMap.getTileHeight());

                        bool intersects = false;

                        int pixelCenX = pixel.X + _theMap.getTileWidth() / 2;
                        int pixelCenY = pixel.Y + _theMap.getTileHeight() / 2;

                        m_boundingTileCircle.Center = new Vector3(pixelCenX, pixelCenY, 0);
                        m_boundingTileCircle.Radius = _theMap.getTileWidth() / 2;

                        if (Map.isTileTypeSquareBlock(tileType, m_frameIndexRendering) || (canUseSlides == false && Map.isTileTypeSlide(tileType)))
                        {
                            m_boundingTileBox = BoundingBox.CreateFromSphere(m_boundingTileCircle);
                            bool localIntersects = m_boundingCharCircle.Intersects(m_boundingTileBox);

                            if(localIntersects)
                            {
                                if (slideAgainstWalls)
                                {
                                    if (GetSlidePositionOffBlock(m_position, newPos, new Point2(pixelCenX, pixelCenY), _theMap.getTileWidth(), m_characterRadius, ref newPos))
                                    {
                                        environmentCollision = true;
                                    }
                                }
                                else
                                {
                                    intersects = true;
                                }
                            }
                        }
                        else if (Map.isTileTypeCircle(tileType))
                        {
                            intersects = m_boundingCharCircle.Intersects(m_boundingTileCircle);
                        }
                        else if (_theMap.isTileTypePixelCollision(tileType))
                        {
                            TileType pixelCheckType = _theMap.GetTileTypePixelCollision(tileType);
                            Point2 topLeft = new Point2(
                                    newPos.X - m_tileSet.getTileWidth() / 2,
                                    newPos.Y - m_tileSet.getTileHeight() / 2);
                            bool localIntersects = m_tileSet.checkForCollision(_theMap.getTileSet(), m_frameIndexCollision, topLeft, (int)pixelCheckType, pixel);

                            if (localIntersects && (!Map.isTileTypeDiagonalWall(tileType) || slideAgainstDiagonalWalls == false))
                            {
                                intersects = localIntersects;
                            }

                        }


                        if (intersects)
                        {
                            environmentCollision = true;
                            newPos = new Point2(m_position);
                        }
                    }
                }

                // 2nd pass for diagonal walls
                if (slideAgainstDiagonalWalls)
                {
                    TestPositionAgainDiagonalWalls(_theMap, minX, minY, maxX, maxY, ref environmentCollision, ref newPos);
                }

                m_boundingCharCircle.Center = new Vector3(newPos.X, newPos.Y, 0);
                m_boundingCharCircle.Radius = m_characterRadius;

                if (_characterArray != null)
                {
                    // Check against other characters
                    for (int c = 0; c < _characterArray.Length; c++)
                    {
                        if (_characterArray[c] == this || _characterArray[c].m_health <= 0 || _characterArray[c].m_finishedLevel)
                        {
                            continue;
                        }

                        BoundingSphere otherCharBounding = new BoundingSphere();
                        otherCharBounding.Center = new
                            Vector3(_characterArray[c].m_position.X, _characterArray[c].m_position.Y, 0);
                        otherCharBounding.Radius = m_characterRadius;

                        if (m_boundingCharCircle.Intersects(otherCharBounding))
                        {
                            if (m_frameIndexRendering == (int)CharacterTile.BADDY)
                            {
                                _characterArray[c].applyDamageDrain(frameTime, GameConstants.baddyDrainAmountPerSec);
                                continue;
                            }
                            // characters are facing each other
                            // This means if characters do occupy the same space (via resurrection or teleport) then they can still move appart 
                            else if (IsFacing(_characterArray[c]))
                            {
                                newPos = new Point2(m_position);
                            }
                            
                        }


                    }

                }


            }      

            if (slideAgainstWalls)
            {
                m_position.X = newPos.X;
                m_position.Y = newPos.Y;
            }
            else
            {
                int realChangeX = newPos.X - m_position.X;
                int percentageX = 100;
                // This will let user get stuck on things instead of just sliding around
                if (change.X * change.X > realChangeX * realChangeX)
                {
                    percentageX = 100 * realChangeX / change.X;
                }

                int realChangeY = newPos.Y - m_position.Y;
                int percentageY = 100;
                // This will let user get stuck on things instead of just sliding around
                if (change.Y * change.Y > realChangeY * realChangeY)
                {
                    percentageY = 100 * realChangeY / change.Y;
                }

                int lowestPercentage = percentageX < percentageY ? percentageX : percentageY;

                // Calculate final position
                m_position.X += change.X * lowestPercentage / 100;
                m_position.Y += change.Y * lowestPercentage / 100;
            }

            SetMovingOnSlideState(movingOnSlide);
            return environmentCollision;
        }

        bool TestPositionAgainDiagonalWalls(Map _theMap, int minX, int minY, int maxX, int maxY, ref bool environmentCollision, ref Point2 newPos)
        {
            bool collidedOnDiagonal = false;

            Point2 result = new Point2();
            Point2 slideBlockCentre = new Point2();

            Point2 localNewPos = new Point2(newPos);
            bool localEnvironmentCollision = environmentCollision;
            
            for (int y = minY; y < maxY; y++)
            {
                for (int x = minX; x < maxX; x++)
                {
                    if (x >= _theMap.getWidth() / _theMap.getTileWidth() &&
                        y >= _theMap.getHeight() / _theMap.getTileHeight())
                    {
                        continue;
                    }

                    TileType tileType = _theMap.getMapTileType(x, y);

                    Point2 pixel = new Point2(
                        x * _theMap.getTileWidth(),
                        y * _theMap.getTileHeight());

                    int pixelCenX = pixel.X + _theMap.getTileWidth() / 2;
                    int pixelCenY = pixel.Y + _theMap.getTileHeight() / 2;

                    m_boundingTileCircle.Center = new Vector3(pixelCenX, pixelCenY, 0);
                    m_boundingTileCircle.Radius = _theMap.getTileWidth() / 2;

                    if (Map.isTileTypeDiagonalWall(tileType) && _theMap.isTileTypePixelCollision(tileType))
                    {
                        TileType pixelCheckType = _theMap.GetTileTypePixelCollision(tileType);
                        Point2 topLeft = new Point2(
                                localNewPos.X - m_tileSet.getTileWidth() / 2,
                                localNewPos.Y - m_tileSet.getTileHeight() / 2);
                        bool localIntersects = m_tileSet.checkForCollision(_theMap.getTileSet(), m_frameIndexCollision, topLeft, (int)pixelCheckType, pixel);

                        if (localIntersects)
                        {
                            if (Map.isTileTypeDiagonalWall(tileType))
                            {
                                if (localEnvironmentCollision || collidedOnDiagonal)
                                {
                                    localNewPos = new Point2(m_position);
                                }
                                else
                                {
                                    if (GetSlidePositionOffDiagonalBlock(tileType, m_position, localNewPos, ref result, pixel))
                                    {
                                        localNewPos = new Point2(result);
                                    }
                                    else
                                    {
                                        slideBlockCentre = new Point2(pixelCenX, pixelCenY);
                                        GetSlidePositionOffBlock(m_position, localNewPos, slideBlockCentre, _theMap.getTileWidth(), m_characterRadius, ref localNewPos);
                                    }

                                    collidedOnDiagonal = true;
                                }

                                localEnvironmentCollision = true;
                            }
                        }
                    } // if diagonal wall
                } // for loop
            } // for loop


            environmentCollision = localEnvironmentCollision;
            newPos.X = localNewPos.X;
            newPos.Y = localNewPos.Y;
            return collidedOnDiagonal;
        }

        bool GetSlidePositionOffBlock(Point2 LastPosition, Point2 newPosition, Point2 tileCentre, int tileSize, float radius, ref Point2 slidePosition)
        {
            bool basicMethod = true;
            if (basicMethod)
            {
                    OgreMaths.Point[] positions = new OgreMaths.Point[4];
                Box l_tile = new Box(new OgreMaths.Point(tileCentre), tileSize + radius * 2);

                l_tile.GetLine((int)Box.Corner.TOP_LEFT);

                positions[0] = new OgreMaths.Point(newPosition);
                positions[0].x = Math.Min(newPosition.X, l_tile.corners[(int)Box.Corner.TOP_LEFT].x);

                positions[1] = new OgreMaths.Point(newPosition);
                positions[1].y = Math.Min(newPosition.Y, l_tile.corners[(int)Box.Corner.TOP_LEFT].y);

                positions[2] = new OgreMaths.Point(newPosition);
                positions[2].x = Math.Max(newPosition.X, l_tile.corners[(int)Box.Corner.BOTTOM_RIGHT].x);

                positions[3] = new OgreMaths.Point(newPosition);
                positions[3].y = Math.Max(newPosition.Y, l_tile.corners[(int)Box.Corner.BOTTOM_RIGHT].y);

                int shortestLengthIndex = 0;
                float shortestLength = OgreMaths.Point.DistanceSquared(new OgreMaths.Point(newPosition), positions[0]);
                for (int i = 0; i < 4; i++)
                {
                    float length = OgreMaths.Point.DistanceSquared(new OgreMaths.Point(newPosition), positions[i]);
                    if (length < shortestLength)
                    {
                        shortestLength = length;
                        shortestLengthIndex = i;
                    }
                }

                slidePosition = new Point2(positions[shortestLengthIndex]);
                return true;
            }
            else
            {

                Box l_tile = new Box(new OgreMaths.Point(tileCentre), tileSize);
                Line l_vectorIn = 
                    new Line(new OgreMaths.Point(LastPosition), 
                    new OgreMaths.Point(newPosition.X, newPosition.Y));

                OgreMaths.Point l_vectorNormalised = l_vectorIn.GetNormalisedVector();
                l_vectorIn.b += l_vectorNormalised.Mult(radius);

                Line lineChosen = new Line();
                OgreMaths.Point intersect = new OgreMaths.Point();
                OgreMaths.Point reflectionPoint = new OgreMaths.Point();
             
                bool intersectionHappened = Box.CalcReflectVectorWithBox(new OgreMaths.Point(tileCentre), tileSize,
                    l_vectorIn, 
                    ref reflectionPoint, ref intersect);

                if (intersectionHappened)
                {
                    OgreMaths.Point reflectVec = reflectionPoint - intersect;
                    reflectVec.Normalise();
                    slidePosition = new Point2(intersect + reflectVec.Mult(radius + 4.5f));
                }
                /*
                if (intersectionHappened)
                {
                    OgreMaths.Point vecDir = l_vectorIn.GetVector();
                    OgreMaths.Point newDirection = lineChosen.GetVector();

                    float temp = newDirection.x;
                    newDirection.x = newDirection.y;
                    newDirection.y = newDirection.x;

                    newDirection.x = Math.Abs(newDirection.x) * vecDir.x / -Math.Abs(vecDir.x);
                    newDirection.y = Math.Abs(newDirection.y) * vecDir.y / -Math.Abs(vecDir.y);
                    OgreMaths.Point directionApplied = intersect + newDirection;



                    slidePosition = new Point2(intersect);//directionApplied);
                    slidePosition = new Point2(directionApplied);
                }
                 */

                return intersectionHappened;
            }
           
        }

        bool GetSlidePositionOffDiagonalBlock(TileType tileType, Point2 position, Point2 newPos, ref Point2 result, Point2 tilePos)
        {
            OgreMaths.Point a = new OgreMaths.Point(position);
            OgreMaths.Point b = new OgreMaths.Point(newPos);

            OgreMaths.Point vector = new OgreMaths.Point(b - a);
            float vecLen = vector.Length() * (float) Math.Sqrt(2.0f);

            if (tileType == TileType.WALL_BOTTOM_RIGHT && position.X < tilePos.X + 80 && position.Y < tilePos.Y + 80)
                //)// && m_rotation > Angles.DEG_45 && m_rotation < Angles.DEG_1 * 225)
            {
                if (Math.Abs(vector.x) >= Math.Abs(vector.y))
                {
                    result.X = position.X + (int)(vecLen / 2);
                    result.Y = position.Y - (int)(vecLen / 2);
                }
                else if (Math.Abs(vector.y) > Math.Abs(vector.x))
                {
                    result.X = position.X - (int)(vecLen / 2);
                    result.Y = position.Y + (int)(vecLen / 2);
                }
            }
            else if (tileType == TileType.WALL_TOP_LEFT && position.X > tilePos.X && position.Y > tilePos.Y)
            {
                if (Math.Abs(vector.x) >= Math.Abs(vector.y))
                {
                    result.X = position.X - (int)(vecLen / 2);
                    result.Y = position.Y + (int)(vecLen / 2);
                }
                else if (Math.Abs(vector.y) > Math.Abs(vector.x))
                {
                    result.X = position.X + (int)(vecLen / 2);
                    result.Y = position.Y - (int)(vecLen / 2);
                }
            }
            else if (tileType == TileType.WALL_BOTTOM_LEFT && position.X > tilePos.X && position.Y < tilePos.Y + 80)
            {
              //  if (m_rotation < Angles.DEG_1 * 225 && m_rotation > Angles.DEG_45)
                {
                    if (Math.Abs(vector.x) >= Math.Abs(vector.y))
                    {
                        result.X = position.X - (int)(vecLen / 2);
                        result.Y = position.Y - (int)(vecLen / 2);
                    }
                    else if (Math.Abs(vector.y) > Math.Abs(vector.x))
                    {
                        result.X = position.X + (int)(vecLen / 2);
                        result.Y = position.Y + (int)(vecLen / 2);
                    }
                }

            }
            else if (tileType == TileType.WALL_TOP_RIGHT && position.X < tilePos.X + 80 && position.Y > tilePos.Y)
            {
                if (Math.Abs(vector.x) >= Math.Abs(vector.y))
                {
                    result.X = position.X + (int)(vecLen / 2);
                    result.Y = position.Y + (int)(vecLen / 2);
                }
                else if (Math.Abs(vector.y) > Math.Abs(vector.x))
                {
                    result.X = position.X - (int)(vecLen / 2);
                    result.Y = position.Y - (int)(vecLen / 2);
                }

            }
            else
            {
                return false;
            }


            return true;
        }

        const float MAX_HEALTH = 100;
        protected void increaseHealth(float _increase)
        {
            m_health += _increase;
            if (m_health > MAX_HEALTH)
            {
                m_health = MAX_HEALTH;
            }
        }

        void increaseScore(int _increase)
        {
            m_score += _increase;
        }

        public bool isCharacterOnTile(Point2 tilePos)
        {
            BoundingBox tile         = new BoundingBox();
            BoundingSphere character = new BoundingSphere();

            character.Center.X = m_position.X;
            character.Center.Y = m_position.Y;
            character.Radius   = m_characterRadius;

            tile.Min.X = Map.m_staticMapInstance.getTileWidth() * tilePos.X;
            tile.Min.Y = Map.m_staticMapInstance.getTileHeight() * tilePos.Y;
            tile.Max.X = tile.Min.X + Map.m_staticMapInstance.getTileWidth();
            tile.Max.Y = tile.Min.Y + Map.m_staticMapInstance.getTileHeight();
            return tile.Intersects(character);
        }

        public TileType GetInventory() {return m_inventory;}

        abstract protected void stopMovement();
        abstract public TileType SetInventory(TileType item);

        virtual protected void SetMovingOnSlideState(bool movingOnSlideState) {}

        protected virtual void increaseFirePower() { }
        protected virtual void increaseFireFequency() { }
        protected virtual void increaseDefence() { }
    }



}
#endif