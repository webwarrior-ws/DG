CREATE TABLE "Users" (
    "UserId" serial PRIMARY KEY
    , "GpsLatitude" FLOAT8
    , "GpsLongitude" FLOAT8
);
CREATE TABLE "Relationships" (
    "UserId" INT NOT NULL
    , "AssigneeId" INT NOT NULL
    , "Closeness" INT NOT NULL
    , PRIMARY KEY ("UserId", "AssigneeId")
    , FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId")
    , FOREIGN KEY ("AssigneeId") REFERENCES "Users" ("UserId")
);
