import { CssUtility } from './songhay-css.utility';
import { CssRedGreenBlue } from '../models/songhay-css-red-green-blue';
import { CssLinearGradientData } from '../models/songhay-css-linear-gradient-data';

describe('CssUtility', () => {
    const utility: CssUtility = new CssUtility();

    it('should instantiate', () => {
        expect(utility).not.toBeNull();
    });
    it('should getColorHex', () => {
        expect(utility.getColorHex('0xEAEAEA')).toBe('#eaeaea');
    });
    it('should getColorRgb', () => {
        expect(utility.getColorRgb(new CssRedGreenBlue(30, 60, 234))).toBe(
            '30, 60, 234'
        );
    });
    it('should getOpacity', () => {
        expect(utility.getOpacity('70')).toBe(0.7);
    });
    it('should getPixelValue', () => {
        expect(utility.getPixelValue(70)).toBe('70px');
    });
    it('should getRgbFromHex', () => {
        const rgb: CssRedGreenBlue = utility.getRgbFromHex('#3F8241');
        expect(rgb.b).toBe(65);
        expect(rgb.g).toBe(130);
        expect(rgb.r).toBe(63);
    });
    it('should getTintedBackground', () => {
        const data: CssLinearGradientData = new CssLinearGradientData(
            '0.3',
            '0x808080',
            'https://myserver.com/background.jpg'
        );
        const expectedCss: string = [
            'linear-gradient(rgba(128, 128, 128, 0.3), ',
            'rgba(128, 128, 128, 0.3)), ',
            `url('https://myserver.com/background.jpg')`
        ].join('');
        expect(utility.getTintedBackground(data)).toBe(expectedCss);
    });
    it('should getUri', () => {
        expect(utility.getUri('https://myserver.com/background.jpg')).toBe(
            `url('https://myserver.com/background.jpg')`
        );
    });
});
