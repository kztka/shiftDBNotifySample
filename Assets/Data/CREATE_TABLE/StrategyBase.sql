CREATE TABLE 
  StrategyBase(
    mapname text not null,
    basename text not null,
    texturePath text,
    textureXScale real,
    textureYScale real,
    x real,
    y real,
    fortressFlg text,
    primary key(
      mapname,
      basename
    )
  );