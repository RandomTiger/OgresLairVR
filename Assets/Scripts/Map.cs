using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Point2
{
    public Point2()
    {
        X = Y = 0;
    }

    public Point2(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X, Y;
}

namespace OgresLair.Game
{
    public enum TileType
    {
        FLOOR,
        WALL,
        WALL_TOP_LEFT,
        WALL_BOTTOM_RIGHT,
        WALL_TOP_RIGHT,
        WALL_BOTTOM_LEFT,

        BREAKABLE_RED,
        BREAKABLE_YELLOW,
        TURRET_RANDOM,
        TURRET_ROOK,
        BREAKABLE_WALL,
        BROKEN_SQUARE,

        PICKUP_GEM_BLUE,
        PICKUP_GEM_GREEN,
        PICKUP_GEM_PINK,
        PICKUP_FIRE_POWER,
        PICKUP_FIRE_SPEED,
        PICKUP_SHEILD,

        PICKUP_SWORD,
        PICKUP_FOOD,
        PICKUP_CROSS,
        USABLE_BOLDER,
        BADDY_SPAWN,
        BADDY_DOOR,

        SLIDE_LEFT,
        SLIDE_RIGHT,
        SLIDE_UP,
        SLIDE_DOWN,
        SLIDE_MID,
        WALL_GREEN,

        GATE_GREEN_SWITCH,
        GATE_GREEN_LEFT,
        GATE_GREEN_MID,
        GATE_GREEN_UP,   
        WALL_MIRROR_VERTICAL,
        WALL_MIRROR_HORIZONTAL,

        GATE_BLUE_SWITCH,
        GATE_BLUE_LEFT,
        GATE_BLUE_MID,
        GATE_BLUE_UP,
        TELEPORT_BLUE,
        TELEPORT_GREEN_SURFACE,

        PIT,
        PLAYER_1,
        PLAYER_2,
        PICKUP_POTION,
        DOOR_VERTICAL,
        DOOR_HORIZONTAL,

        PICKUP_MUSHROOM_GOOD,
        PICKUP_MUSHROOM_BAD,
        PICKUP_PAULS_SHOE,
        KEY_WHITE,

        GREEN_HAND_RIGHT,
        GREEN_HAND_LEFT,

        WALL_MOVING_START,
        WALL_MOVING_END,

        OGRE,
        GRASS,
        TREE,

        TILE_ONE = TREE + 20,
        TILE_LAST  = TILE_ONE + 49,

        TILE_TRANSPARENT = -1,
        NO_OVERRIDE = -2,
        OUT_OF_BOUNDS = -3,
        NONE = -4,
    };

    public class Map : MonoBehaviour
    {
        enum SpecialType
        {
            NONE,
            RATTLING,
        };

        public static Map m_staticMapInstance = null;

        public Point2 m_playerOneStartPos;
        public Point2 m_playerTwoStartPos;
        public Point2 m_ogreStartPos;

        public bool m_ogreActiveOnMap;

        TileType[] m_mapData;
        TileType[] m_mapDataLayer1;
        TileType[] m_mapDataLayer2;

        int[] m_mapExtraData;
        SpecialType[] m_mapDataSpecial;

        int m_numTilesWide;
        int m_numTilesHigh;

        bool m_greenGateVertical;
        bool m_blueGateVertical;

        public ArrayList m_baddyArray  = null;
        ArrayList m_turretArray = null;

        public static int ms_tileWidth;

        private List<Point2> m_teleporterList = new List<Point2>();
        public List<Point2> m_movingWallsList = new List<Point2>();

        TileType m_outsideWallIndex = TileType.WALL;

        public string Name;
        public int Num;

        public void Start()
        {
            LoadKevinMartinMap(Name, Num);
        }

        public void LoadMap(String mapName)
        {
            loadMapData(mapName);
            preprocessMapData();
        }

        public void LoadKevinMartinMap(String fileName, int levelNum)
        {
            loadLevelFromBlock(fileName, levelNum);
            //saveLevel("map" + _levelNum + ".MAP");


            if (levelNum == 15)
            {
                int x = 36;
                int y = 15;
                int index = x + y * m_numTilesWide;
                m_mapData[index] = TileType.OGRE;
            }

            if (levelNum == 16)
            {
                int x = 22;
                int y = 12;
                int index = x + y * m_numTilesWide;
                m_mapData[index] = TileType.OGRE;
            }

            preprocessMapData();
        }

        public void SetOutsideTile(TileType type)
        {
            m_outsideWallIndex = type;
        }

        Point2 getPosition(int _index)
        {
            return new Point2(_index % m_numTilesWide, _index / m_numTilesWide);
        }

        void loadMapData(String _mapName)
        {
            FileStream fileSteam = 
                //Controller.content.Load<FileStream>(_mapName);
                File.Open(_mapName, FileMode.Open, FileAccess.Read);

            m_numTilesWide = fileSteam.ReadByte();
            m_numTilesHigh = fileSteam.ReadByte();

            m_mapData       = new TileType[m_numTilesWide * m_numTilesHigh];
            m_mapDataLayer1 = new TileType[m_numTilesWide * m_numTilesHigh];
            m_mapDataLayer2 = new TileType[m_numTilesWide * m_numTilesHigh];

            for (int i = 0; i < m_mapData.Length; i++)
            {
                m_mapData[i] = (TileType)fileSteam.ReadByte();
            }

            for (int i = 0; i < m_mapDataLayer1.Length; i++)
            {
                m_mapDataLayer1[i] = (TileType)fileSteam.ReadByte();
            }
            for (int i = 0; i < m_mapDataLayer2.Length; i++)
            {
                m_mapDataLayer2[i] = (TileType)fileSteam.ReadByte();
            }

            // extract teleport data
            for (int i = 0; i < m_mapData.Length; i++)
            {
                if (m_mapData[i] == TileType.TELEPORT_BLUE)
                {
                    m_teleporterList.Add(new Point2(0, 0));
                }
            }

            for (int j = 0; j < m_numTilesHigh; j++)
            {
                for (int i = 0; i < m_numTilesWide; i++)
                {
                    int index = i + j * m_numTilesWide;
                    if (m_mapData[index] == TileType.TELEPORT_BLUE)
                    {
                        int teleporterIndex = m_mapDataLayer1[index] - TileType.TILE_ONE;

                        if (teleporterIndex >= 0 && teleporterIndex < m_teleporterList.Count)
                        {
                            m_teleporterList[teleporterIndex] = new Point2(i, j);
                        }
                    }
                }
            }

            for (int i = 0; i < m_mapData.Length; i++)
            {
                if (m_mapData[i] == TileType.KEY_WHITE)
                {

                 //   m_mapDataLayer1[i] = TileType.TILE_ONE + (int)m_mapData[i] - 200;
                    m_mapData[i] = m_mapDataLayer1[i];// TileType.TILE_ONE + (int)m_mapData[i] - 200;
                }
            }

            fileSteam.Close();
        }

        void saveLevel(String _filename)
        {
            FileStream fileSteam =
                File.Open(_filename, FileMode.Create, FileAccess.Write);

            fileSteam.WriteByte((byte) m_numTilesWide);
            fileSteam.WriteByte((byte) m_numTilesHigh);

            for (int i = 0; i < m_mapData.Length; i++)
            {
                fileSteam.WriteByte((byte)m_mapData[i]);
            }

            for (int i = 0; i < m_mapDataLayer1.Length; i++)
            {
                fileSteam.WriteByte((byte)m_mapDataLayer1[i]);
            }

            fileSteam.Close();
        }

        public GameObject [] Prefabs;

        void preprocessMapData()
        {
            m_mapExtraData = new int[m_numTilesWide * m_numTilesHigh];
            m_mapDataSpecial = new SpecialType[m_numTilesWide * m_numTilesHigh];

            m_baddyArray = new ArrayList();
            m_turretArray = new ArrayList();

            m_ogreActiveOnMap = false;

            // Find player positions
            for (int i = 0; i < m_mapData.Length; i++)
            {
                TileType tileType = (TileType)m_mapData[i];

                m_mapDataSpecial[i] = SpecialType.NONE;

                if (tileType == TileType.PLAYER_1)
                {
                    m_playerOneStartPos = getPosition(i);
                    m_mapData[i] = TileType.FLOOR;
                }

                if (tileType == TileType.PLAYER_2)
                {
                    m_playerTwoStartPos = getPosition(i);
                    m_mapData[i] = TileType.FLOOR;
                }

                if (tileType == TileType.OGRE)
                {
                    m_ogreStartPos = getPosition(i);
                    m_mapData[i] = TileType.FLOOR;

                    m_ogreActiveOnMap = true;
                }
            }

            // Map borders
            for(int i = 0; i < m_numTilesWide; i++)
            {
                Instantiate(Prefabs[(int)TileType.WALL], new Vector3(i, 0, -1), Quaternion.identity, transform);
                Instantiate(Prefabs[(int)TileType.WALL], new Vector3(i, 0, m_numTilesHigh), Quaternion.identity, transform);
            }

            for (int i = 0; i < m_numTilesHigh; i++)
            {
                Instantiate(Prefabs[(int)TileType.WALL], new Vector3(-1, 0, i), Quaternion.identity, transform);
                Instantiate(Prefabs[(int)TileType.WALL], new Vector3(m_numTilesWide, 0, i), Quaternion.identity, transform);
            }

            // Find special types
            for (int y = 0; y < m_numTilesHigh; y++)
            {
                for (int x = 0; x < m_numTilesWide; x++)
                {
                    TileType tileType = getMapTileType(x,y);

                    switch(tileType)
                    {
                        case TileType.WALL:
                        case TileType.BREAKABLE_RED:
                        case TileType.BREAKABLE_YELLOW:
                        case TileType.USABLE_BOLDER:
                        case TileType.DOOR_VERTICAL:
                        case TileType.DOOR_HORIZONTAL:
                            {
                                Instantiate(Prefabs[(int)tileType], new Vector3(x, 0, y), Quaternion.identity, transform);
                            }
                            break;
                    }

                    if (tileType == TileType.BADDY_SPAWN)
                    {
                    //    m_baddyArray.Add(new BaddyGenerator(this, new Point2(x, y)));
                    }

                    // if destructable state hit points
                    m_mapExtraData[x + y * m_numTilesWide] = 100;

                    // if teleporter state index

                    // if key door or 

                    Point2 pos = new Point2(x, y);
                    /*
                    if (tileType == TileType.TURRET_RANDOM)
                    {
                        m_turretArray.Add(new TurretRandom(this, pos));
                    }

                    if (tileType == TileType.TURRET_ROOK)
                    {
                        m_turretArray.Add(new TurretRook(this, pos));
                    }

                    if (tileType == TileType.WALL_MOVING_START)
                    {
                        m_movingWallsList.Add(new Point2(x, y));
                    }*/
                }
            }

            m_staticMapInstance = this;
        }

        public void SetHubLevelUnlock(int level)
        {
            for (int i = 0; i < m_mapData.Length; i++)
            {
                if (m_mapData[i] == TileType.DOOR_HORIZONTAL || m_mapData[i] == TileType.DOOR_VERTICAL)
                {
                    if (m_mapDataLayer1[i] >= TileType.TILE_ONE && m_mapDataLayer1[i] <= TileType.TILE_LAST)
                    {
                        if (m_mapDataLayer1[i] - TileType.TILE_ONE < level)
                        {
                            m_mapData[i] = TileType.FLOOR;
                        }
                    }
                }
            }
        }

        public void update()
        {
            for(int i = 0; i < getNumTilesWide() * getNumTilesHigh(); i++)
            {
                m_mapDataSpecial[i] = SpecialType.NONE;
            }
            /*
            for (int i = 0; i < m_baddyArray.Count; i++)
            {
                ((BaddyGenerator)m_baddyArray[i]).update(frameTime, this, players);
            }

            for (int i = 0; i < m_turretArray.Count; i++)
            {
                ((Turret)m_turretArray[i]).update(frameTime, this, players);
            }
            */
            m_wallMove.Update(Time.deltaTime);
        }

        bool tileActive(TileType tileType)
        {
            if(tileType == TileType.GATE_GREEN_LEFT && m_greenGateVertical ||
               tileType == TileType.GATE_BLUE_LEFT  && m_blueGateVertical  ||
               tileType == TileType.GATE_GREEN_UP  && !m_greenGateVertical ||
               tileType == TileType.GATE_BLUE_UP   && !m_blueGateVertical)
            {
                return false;
            }
                
            return true;
        }

        public int getNumTilesWide()
        {
            return m_numTilesWide;
        }

        public int getNumTilesHigh()
        {
            return m_numTilesHigh;
        }

        public float getTileWidth()
        {
            return 1;
        }

        public int getMapTile(int _x, int _y)
        {
            return (int) m_mapData[_x + _y * m_numTilesWide];
        }

        private TileType getMapTileLayer2(int _x, int _y)
        {
            if (m_mapDataLayer2 != null)
            {
                return m_mapDataLayer2[_x + _y * m_numTilesWide];
            }

            return TileType.FLOOR;
        }


        public TileType getKeyNum(int tileX, int tileY)
        {
            if(!isDoor(getMapTileType(tileX, tileY)))
            {
                return TileType.OUT_OF_BOUNDS;
            }

            return m_mapDataLayer1[tileX + tileY * m_numTilesWide];
        }

        public TileType getMapTileType(int _x, int _y)
        {
            if (_x < 0 || _x >= m_numTilesWide ||
                _y < 0 || _y >= m_numTilesHigh)
            {
                return TileType.OUT_OF_BOUNDS;
            }

            return m_mapData[_x + _y * m_numTilesWide];
        }

        public void setMapTileTypeFromPixels(int _x, int _y, TileType _newType)
        {
            if (_newType == TileType.NONE)
            {
                _newType = TileType.FLOOR;
            }

            setMapTileType(_x, _y, _newType);
        }

        public bool IsValidTile(int x, int y)
        {
            return x >= 0 && x < m_numTilesWide && y >= 0 && y < m_numTilesHigh;
        }

        public void setMapTileType(int _x, int _y, TileType _newType)
        {
            m_mapData[_x + _y * m_numTilesWide] = _newType; 
        }

        public void setMapTileRattle(int _x, int _y)
        {
            m_mapDataSpecial[_x + _y * m_numTilesWide] = SpecialType.RATTLING; 
        }


        public void toggleGreenGates()
        {
            m_greenGateVertical = !m_greenGateVertical;
        }

        public void toggleBlueGates()
        {
            m_blueGateVertical = !m_blueGateVertical;
        }




        void loadLevelFromBlock(String _mapfile, int _levelNum)
        {
            // always the same
            m_numTilesWide = 62;
            m_numTilesHigh = 32;
            int numLevels = 17;

            TextAsset bindata = Resources.Load(_mapfile) as TextAsset;
            Stream fileSteam = new MemoryStream(bindata.bytes);

           // FileStream fileSteam = 
                //Controller.content.Load<FileStream>(_mapfile);
           //     File.Open(_mapfile, FileMode.Open, FileAccess.Read);
            m_mapData       = new TileType[m_numTilesWide * m_numTilesHigh];
            m_mapDataLayer1 = new TileType[m_numTilesWide * m_numTilesHigh];

           // const int MONSTER_GEN = 0; // max 4 in editor
           // const int DOOR_POS    = 16;  // max 50 doors * 4 (data size)
           // const int TRANSPORT   = 216; // max9 blue in editor

            int mapOffset =  660;
            int sizeOfEachLevel = (int) fileSteam.Length / numLevels;

            byte [] preMapData = new byte[mapOffset];
            fileSteam.Seek(sizeOfEachLevel * _levelNum, 0);
            fileSteam.Read(preMapData, 0, mapOffset);


            const int MONSTER_GEN_ARRAY_SIZE = 16;
            byte[] monsterGenArray = new byte[MONSTER_GEN_ARRAY_SIZE];
            fileSteam.Seek(sizeOfEachLevel * _levelNum, 0);
            fileSteam.Read(monsterGenArray, 0, MONSTER_GEN_ARRAY_SIZE);

            const int DOOR_POS_ARRAY_SIZE = 200;
            byte[] doorPosArray = new byte[DOOR_POS_ARRAY_SIZE];
            fileSteam.Seek(sizeOfEachLevel * _levelNum + MONSTER_GEN_ARRAY_SIZE, 0);
            fileSteam.Read(doorPosArray, 0, DOOR_POS_ARRAY_SIZE);

            const int TRANSPORT_ARRAY_SIZE = 20;
            byte[] tempTransportArray = new byte[TRANSPORT_ARRAY_SIZE];
            fileSteam.Seek(sizeOfEachLevel * _levelNum + MONSTER_GEN_ARRAY_SIZE + DOOR_POS_ARRAY_SIZE, 0);
            fileSteam.Read(tempTransportArray, 0, TRANSPORT_ARRAY_SIZE);
            
            for (int i = 0; i < monsterGenArray.Length; i+=2)
            {
                if(monsterGenArray[i + 0] != 0 && monsterGenArray[i + 1] != 0)
                {
                    monsterGenArray[i + 0] -= 4;
                    monsterGenArray[i + 1] = (byte) (m_numTilesHigh + 5 - monsterGenArray[i + 1]);
                }
            }

          
            for (int i = 0; i < tempTransportArray.Length; i += 2)
            {
                // If its a valid transporter
                if (tempTransportArray[i + 0] != 0 && tempTransportArray[i + 1] != 0)
                {
                    // Decrypt
                    tempTransportArray[i + 0] -= 4;
                    tempTransportArray[i + 1] = (byte)(m_numTilesHigh + 5 - tempTransportArray[i + 1]);

                    m_teleporterList.Add(new Point2(tempTransportArray[i + 0], tempTransportArray[i + 1]));
                }                
            }

            tempTransportArray = null;
      
            int doorCount = 0;
            for (int i = 0; i < doorPosArray.Length; i += 4)
            {
                if (doorPosArray[i + 0] != 0 && doorPosArray[i + 1] != 0)
                {
                    int position = doorPosArray[i + 0] + (256 * doorPosArray[i + 1]);

                    doorPosArray[i + 0] = (byte)((position % 70) - 4);
                    doorPosArray[i + 1] = (byte)(m_numTilesHigh + 5 - (position / 70));
                    doorCount++;
                }
                else
                {
                    doorPosArray[i + 0] = doorPosArray[i + 1] = 255;
                }
            }

            // Read the actual tile data
            fileSteam.Seek(sizeOfEachLevel * _levelNum + mapOffset, 0);

            bool read_8_150 = true;
          //  int lastRead;

            for (int y = m_numTilesHigh - 1; y >= 0; y--)
            {
                int count = 0;
                for (int x = 0; x < m_numTilesWide; x++)
                {
                    byte byteRead = (byte)fileSteam.ReadByte();

                    if (byteRead == 150 && read_8_150 == false)
                    {
                        
                        count++;
                        if (count == 8)
                        {
                            read_8_150 = true;
                            x = 0;
                            byteRead = (byte)fileSteam.ReadByte();
                        }
                        else
                        {
                               continue;
                        }
                    }
                 
                //    if (m_mapData[y * m_numTilesWide + x] == TileType.FLOOR)
                    {
                        m_mapData[y * m_numTilesWide + x] = convertBlockByteToTileType(byteRead);
                    }

                }

                read_8_150 = false;
            }
            fileSteam.Close();   
         
            // Process top layer
            
            for(int i = 0; i < m_mapDataLayer1.Length; i++)
            {
                m_mapDataLayer1[i] = TileType.TILE_TRANSPARENT;
            }

            for (int t = 0; t < m_teleporterList.Count; t++)
            {
                int x = m_teleporterList[t].X; 
                int y = m_teleporterList[t].Y;

                m_mapDataLayer1[y * m_numTilesWide + x] = TileType.TILE_ONE + t;     
            }

            for (int i = 0; i < doorPosArray.Length; i += 4)
            {
                if (doorPosArray[i + 0] != 255 && doorPosArray[i + 1] != 255)
                {
                    int position = doorPosArray[i + 0] + (256 * doorPosArray[i + 1]);

                    byte x = doorPosArray[i + 0];
                    byte y = doorPosArray[i + 1];

                    m_mapDataLayer1[y * m_numTilesWide + x] = TileType.TILE_ONE + (i / 4);  
         
                }
            }

            for (int i = 0; i < m_mapData.Length; i++)
            {
                if (((int) m_mapData[i]) >= 200 && ((int) m_mapData[i]) <= 250)
                {
                    
                    m_mapDataLayer1[i] = TileType.TILE_ONE + (int) m_mapData[i] - 200;
                    m_mapData[i] = TileType.TILE_ONE + (int)m_mapData[i] - 200;
                }
            }
           
        }

        TileType convertBlockByteToTileType(int _byte)
        {
           
            if (_byte == 150)
            {
                return TileType.WALL;
            }

            if (_byte == 192)
            {
                return TileType.WALL_TOP_LEFT;
            }
            if (_byte == 193)
            {
                return TileType.WALL_BOTTOM_RIGHT;
            }
            if (_byte == 194)
            {
                return TileType.WALL_TOP_RIGHT;
            }
            if (_byte == 195)
            {
                return TileType.WALL_BOTTOM_LEFT;
            }

            if (_byte == 109)
            {
                return TileType.TURRET_RANDOM;
            }
            if (_byte == 119)
            {
                return TileType.TURRET_ROOK;
            }

            if (_byte == 151)
            {
                return TileType.BROKEN_SQUARE;
            }

            



            if (_byte == 174)
            {
                return TileType.PLAYER_1;
            }
            if (_byte == 175)
            {
                return TileType.PLAYER_2;
            }
            if (_byte == 173)
            {
                return TileType.PIT;
            }
            if (_byte == 69)
            {
                return TileType.BREAKABLE_YELLOW;
            }

            

           if (_byte == 9)
            {
                return TileType.BREAKABLE_RED;
            }

            if (_byte == 143)
            {
                return TileType.SLIDE_DOWN;
            }

            if (_byte == 142)
            {
                return TileType.SLIDE_UP;
            }

            if (_byte == 144)
            {
                return TileType.SLIDE_LEFT;
            }

            if (_byte == 145)
            {
                return TileType.SLIDE_RIGHT;
            }

            if (_byte == 163)
            {
                return TileType.PICKUP_GEM_BLUE;
            }
            if (_byte == 164)
            {
                return TileType.PICKUP_GEM_GREEN;
            }
            if (_byte == 165)
            {
                return TileType.PICKUP_GEM_PINK;
            }

            if (_byte == 198)
            {
                return TileType.TELEPORT_GREEN_SURFACE;
            }
            if (_byte == 172)
            {
                return TileType.TELEPORT_BLUE;
            }

            if (_byte == 156)
            {
                return TileType.PICKUP_FOOD;
            }

            if (_byte == 168)
            {
                return TileType.WALL_MIRROR_HORIZONTAL;
            }
            if (_byte == 169)
            {
                return TileType.WALL_MIRROR_VERTICAL;
            }

            if (_byte == 19)
            {
                return TileType.BREAKABLE_WALL;
            }
            /*
            if (_byte == 29)
            {
                return TileType.WALL_THIN_LEFT;
            }
            if (_byte == 39)
            {
                return TileType.WALL_THIN_RIGHT;
            }
            if (_byte == 49)
            {
                return TileType.WALL_THIN_TOP;
            }
            if (_byte == 59)
            {
                return TileType.WALL_THIN_BOTTOM;
            }
            */
            if (_byte == 152)
            {
                return TileType.USABLE_BOLDER;
            }
            if (_byte == 155)
            {
                return TileType.BADDY_SPAWN;
            }

            if (_byte == 162)
            {
                return TileType.PICKUP_CROSS;
            }

            if (_byte == 166)
            {
                return TileType.PICKUP_SHEILD;
            }

            if (_byte == 167)
            {
                return TileType.PICKUP_FIRE_POWER;
            }

            if (_byte == 170)
            {
                return TileType.PICKUP_FIRE_SPEED;
            }

            if (_byte == 171)
            {
                return TileType.PICKUP_POTION;
            }

            if (_byte == 157)
            {
                return TileType.GATE_GREEN_SWITCH;
            }

            if (_byte >= 200 && _byte <= 230)
            {
             //   return TileType.KEY_1 + _byte - 200;
            }

            if (_byte == 154)
            {
                return TileType.DOOR_VERTICAL;
            }

            if (_byte == 153)
            {
                return TileType.DOOR_HORIZONTAL;
            }

            if (_byte == 180)
            {
                return TileType.GATE_BLUE_SWITCH;
            }

            if (_byte == 181)
            {
            
            }


            if (_byte == 182)
            {
                return TileType.WALL_GREEN;
            }

            if (_byte == 187)
            {
                // Grabbing hand right
            }
            if (_byte == 183)
            {
                // Grabbing hand left
            }

            if (_byte == 255)
            {
                return TileType.PICKUP_SWORD;
            }

            if (_byte == 191)
            {
                return TileType.BADDY_DOOR;
            }

            if (_byte == 139)
            {
                return TileType.WALL_MOVING_START;
            }

            if (_byte == 140)
            {
                return TileType.WALL_MOVING_END;
            }


            if (_byte == 158)
            {
                return TileType.GATE_GREEN_MID;
            }

            if (_byte == 176)
            {
                return TileType.GATE_BLUE_MID;
            }

            if (_byte == 178)
            {
                return TileType.GATE_BLUE_UP;
            }
            if (_byte == 179)
            {
                return TileType.GATE_BLUE_LEFT;
            }

            if (_byte == 160)
            {
                return TileType.GATE_GREEN_UP;
            }
            if (_byte == 161)
            {
                return TileType.GATE_GREEN_LEFT;
            }
         
            if (_byte == 187)
            {
                return TileType.GREEN_HAND_RIGHT;
            }

            if (_byte == 183)
            {
                return TileType.GREEN_HAND_LEFT;
            }

            
            if (_byte == 100)
            {
                // map 4, assuming a typo or unimplemented tile
                return TileType.FLOOR;
            }

            if (_byte == 181)
            {
                // map 12
                return TileType.WALL_GREEN;
            }

            if (_byte == 141)
            {
                return TileType.SLIDE_MID;
            }

            if (_byte == 196 || _byte == 197) // sign
            {
                return TileType.FLOOR;
            }

            if (_byte != 0 && (_byte < 200 || _byte > 250))
            {
                //int test = 0;
            }

            if (_byte == 0)
            {
                return TileType.FLOOR;
            }

            return (TileType) _byte;
        }

        public void applyDamage(int x, int y)
        {
            TileType type = getMapTileType(x, y);

            if (type == TileType.BREAKABLE_WALL ||
                type == TileType.BREAKABLE_RED ||
                type == TileType.BREAKABLE_YELLOW)
            {
                const int DAMAGE = 20;
                m_mapExtraData[x + y * m_numTilesWide] -= DAMAGE;
                if (m_mapExtraData[x + y * m_numTilesWide] <= 0)
                {
                    m_mapData[x + y * m_numTilesWide] = TileType.BROKEN_SQUARE;
                    /*
                    Sound.Play("SFX_Destroy_Box");

                    Game.getOgre().CreatePathfindMap(this);
                    */
                }
                else
                {
                    //Sound.Play("SFX_Destroy_Box");
                }
            }
        }

        public static bool isDoor(TileType _type)
        {
            return _type == TileType.DOOR_VERTICAL ||
                    _type == TileType.DOOR_HORIZONTAL;
        }

        public static bool IsKey(TileType _type)
        {
            return _type >= TileType.TILE_ONE && _type <= TileType.TILE_LAST;
        }

        public static bool isPickup(TileType _type)
        {
            return isPickupHold(_type) || isPickupToggle(_type);
        }

        public static bool isPickupHold(TileType _type)
        {
            return
                _type == TileType.PICKUP_GEM_BLUE ||
                _type == TileType.PICKUP_GEM_GREEN ||
                _type == TileType.PICKUP_GEM_PINK ||

                _type == TileType.PICKUP_FIRE_POWER ||
                _type == TileType.PICKUP_FIRE_SPEED ||
                _type == TileType.PICKUP_SHEILD ||
                _type == TileType.PICKUP_FOOD ||
                _type == TileType.PICKUP_POTION ||

                _type == TileType.PICKUP_MUSHROOM_GOOD ||
                _type == TileType.PICKUP_MUSHROOM_BAD;
        }

        public static bool isPickupToggle(TileType _type)
        {
            return
                _type == TileType.PICKUP_SWORD ||
                _type == TileType.PICKUP_CROSS ||
                Map.IsKey(_type);
        }

        public static bool isTileTypeCircleNotTurrent(TileType _type)
        {
            return
                    _type == TileType.GATE_BLUE_MID ||
                    _type == TileType.GATE_GREEN_MID ||
                    _type == TileType.USABLE_BOLDER;
        }

        public static bool isTileTypeCircle(TileType _type)
        {
            return
                    _type == TileType.TURRET_RANDOM ||
                    _type == TileType.TURRET_ROOK ||
                    _type == TileType.USABLE_BOLDER;
        }

        public static bool isTileTypeSlide(TileType _type)
        {
            return
                _type == TileType.SLIDE_DOWN ||
                _type == TileType.SLIDE_UP ||
                _type == TileType.SLIDE_LEFT ||
                _type == TileType.SLIDE_RIGHT;
        }

        public static bool isTileTypeTeleport(TileType _type)
        {
            return
                _type == TileType.TELEPORT_BLUE ||
                _type == TileType.TELEPORT_GREEN_SURFACE;
        }

        public TileType GetTileTypePixelCollision(TileType type)
        {
            if(type == TileType.GATE_BLUE_MID)
            {
                return m_blueGateVertical ? TileType.GATE_BLUE_UP : TileType.GATE_BLUE_LEFT;
            }
            if (type == TileType.GATE_GREEN_MID)
            {
                return m_greenGateVertical ? TileType.GATE_GREEN_UP : TileType.GATE_GREEN_LEFT;
            }

            return type;
        }

        public bool isTileTypePixelCollision(TileType _type)
        {
            if(_type == TileType.GATE_GREEN_LEFT)
            {
                return !m_greenGateVertical;
            }

            if(_type == TileType.GATE_GREEN_UP)
            {
                return m_greenGateVertical;
            }

            if(_type == TileType.GATE_BLUE_LEFT)
            {
                return !m_blueGateVertical;
            }

            if(_type == TileType.GATE_BLUE_UP)
            {
                return m_blueGateVertical;
            }
           

            return
            _type == TileType.WALL_BOTTOM_LEFT ||
            _type == TileType.WALL_BOTTOM_RIGHT ||
            _type == TileType.WALL_TOP_LEFT ||
            _type == TileType.WALL_TOP_RIGHT ||

            _type == TileType.DOOR_VERTICAL ||
            _type == TileType.DOOR_HORIZONTAL ||

            _type == TileType.GATE_BLUE_MID ||
            _type == TileType.GATE_GREEN_MID ||

            _type == TileType.GATE_GREEN_SWITCH ||
            _type == TileType.GATE_BLUE_SWITCH;
        }

        public static bool isTileTypeRebound(TileType _type)
        {
            return  _type == TileType.WALL_MIRROR_HORIZONTAL ||
                    _type == TileType.WALL_MIRROR_VERTICAL;
        }

        public static bool isTileTypeWall(TileType _type)
        {
            return _type == TileType.WALL ||
                   _type == TileType.WALL_TOP_LEFT ||
                   _type == TileType.WALL_BOTTOM_LEFT ||
                   _type == TileType.WALL_TOP_RIGHT ||
                   _type == TileType.WALL_BOTTOM_RIGHT ||
                   _type == TileType.WALL_GREEN;
        }

        public static bool isTileTypeSquareBlock(TileType _type, int _characterType)
        {
            return isTileTypeSquareBlockForWeaponFire(_type, _characterType) ||  _type == TileType.BADDY_DOOR;
        }

        public static bool isTileTypeSquareBlockForWeaponFire(TileType _type, int _characterType)
        {
            // Baddys treat slides like walls
          //  if (_characterType == (int)CharacterTile.BADDY)
            {
                if (_type == TileType.BADDY_DOOR)
                {
                    return false;
                }

                if (_type == TileType.TELEPORT_BLUE ||
                    _type == TileType.TELEPORT_GREEN_SURFACE ||
                    _type == TileType.PIT)
                {
                    return true;
                }

                if (_type == TileType.SLIDE_DOWN ||
                    _type == TileType.SLIDE_UP ||
                    _type == TileType.SLIDE_LEFT ||
                    _type == TileType.SLIDE_RIGHT)
                {
                    return true;
                }
            }

            return _type == TileType.WALL ||
                _type == TileType.WALL_MIRROR_HORIZONTAL ||
                _type == TileType.WALL_MIRROR_VERTICAL ||
                _type == TileType.WALL_GREEN ||
                isTileTypeSquareBlockDestructable(_type);
        }

        public static bool isTileTypeDiagonalWall(TileType _type)
        {
            return _type == TileType.WALL_BOTTOM_LEFT ||
                    _type == TileType.WALL_BOTTOM_RIGHT ||
                    _type == TileType.WALL_TOP_LEFT ||
                    _type == TileType.WALL_TOP_RIGHT;
        }

        public static bool isTileTypeSquareBlockDestructable(TileType _type)
        {
            return  _type == TileType.BREAKABLE_WALL ||
                    _type == TileType.BREAKABLE_RED ||
                    _type == TileType.BREAKABLE_YELLOW;
        }

        public int getNextTransporter(int tileX, int tileY)
        {
            for (int i = 0; i < m_teleporterList.Count; i++)
            {
                if(m_teleporterList[i].X == tileX && m_teleporterList[i].Y == tileY)
                {
                    return (i + 1) % m_teleporterList.Count;
                }

            }

            return -1;
        }

        public int getTransporterX(int _transportTo)
        {
            return m_teleporterList[_transportTo].X;
        }

        public int getTransporterY(int _transportTo)
        {
            return m_teleporterList[_transportTo].Y;
        }

        public int GetTileNumberData(int tileX, int tileY)
        {
            return GetTileNumberData(tileX + getNumTilesWide() * tileY);
        }

        private int GetTileNumberData(int tileNum)
        {
            if (m_mapDataLayer1[tileNum] >= TileType.TILE_ONE && m_mapDataLayer1[tileNum] <= TileType.TILE_LAST)
            {
                return m_mapDataLayer1[tileNum] - TileType.TILE_ONE + 1;
            }

            return 0;
        }

        public int KillAllBaddiesInRadius(Point2 position, float distance)
        {
            int countKilled = 0;

            for (int i = 0; i < m_baddyArray.Count; i++)
            {
            //    countKilled += ((BaddyGenerator)m_baddyArray[i]).KillAllInRadius(position, distance);
            }

            return countKilled;
        }

        public void PlayMapMusic()
        {
          //  Sound.Play("Music_Gameplay1");
        }

        public class WallMove
        {
            public WallMove(float speed)
            {
                m_speed = speed;
                m_direction = 1.0f;
            }
            public void Update(float gameTime)
            {
                m_pos += m_direction * gameTime;
                if (m_pos > 1.0f)
                {
                    m_pos = 2.0f - m_pos;
                    m_direction *= -1.0f;
                }
                if (m_pos < 0.0f)
                {
                    m_pos = -m_pos;
                    m_direction *= -1.0f;
                }
            }

            public float GetValue() { return m_pos; }

            private float m_pos;
            private float m_speed;
            private float m_direction;
        };

        public WallMove m_wallMove = new WallMove(0.01f);
    }
}
