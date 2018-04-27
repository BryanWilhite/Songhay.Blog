import { MathUtility } from './songhay-math.utility';

describe('MathUtility', () => {
    const utility: MathUtility = new MathUtility();

    it('should instantiate', () => {
        expect(utility).not.toBeNull();
    });
    it('should round a number', () => {
        expect(utility.round(4.35, 1)).toBe(4.4);
    });
});
