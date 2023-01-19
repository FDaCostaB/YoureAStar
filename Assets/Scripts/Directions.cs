using System;

[Flags]
public enum Directions{
    NORTHWEST = 0b00000001,
    NORTH = 0b00000010,
    NORTHEAST=0b00000100,
    WEST = 0b00001000,
    EAST = 0b00010000,
    SOUTHWEST=0b00100000,
    SOUTH=0b01000000,
    SOUTHEAST=0b10000000
}
public static class MaskFree{
    public static byte[] maskFree = {0b11110100, 0b11111101, 0b11101001, 0b11110111, 0b11101111, 0b10010111, 0b10111111, 0b00101111};
}